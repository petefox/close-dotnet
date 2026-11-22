using System.Net;
using System.Security.Cryptography;
using System.Text;
using Close.DotNet.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Close.DotNet.Tests;

public sealed class WebhookEndpointTests
{
    private const string Key = "01020304";

    [Fact]
    public async Task VerifiedPostDeliversTheOriginalBodyAndEventToTheHandler()
    {
        var token = TestContext.Current.CancellationToken;
        const string payload = """{ "event": { "id": "ev_test", "data": { "name": "André" } } }""";
        var received = new TaskCompletionSource<(string? Id, string? Path, byte[] Body)>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var app = await StartAsync((request, _) =>
        {
            received.TrySetResult((request.Event.Id, request.Context.Request.Path.Value, request.RawBody.ToArray()));
            return Task.CompletedTask;
        }, token);
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        using var post = SignedPost(payload);
        using var response = await http.SendAsync(post, token);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var delivery = await received.Task.WaitAsync(token);
        Assert.Equal("ev_test", delivery.Id);
        Assert.Equal("/hooks/close", delivery.Path);
        Assert.Equal(Encoding.UTF8.GetBytes(payload), delivery.Body);
    }

    [Theory]
    [InlineData("missing", HttpStatusCode.Unauthorized)]
    [InlineData("changed", HttpStatusCode.Unauthorized)]
    [InlineData("duplicate", HttpStatusCode.Unauthorized)]
    [InlineData("malformed", HttpStatusCode.BadRequest)]
    [InlineData("no-event", HttpStatusCode.BadRequest)]
    [InlineData("stale", HttpStatusCode.Unauthorized)]
    public async Task InvalidDeliveriesAreRejectedBeforeCallingTheHandler(string variant, HttpStatusCode expected)
    {
        var token = TestContext.Current.CancellationToken;
        var calls = 0;
        await using var app = await StartAsync((_, _) =>
        {
            Interlocked.Increment(ref calls);
            return Task.CompletedTask;
        }, token, new() { SignatureMaxAge = TimeSpan.FromMinutes(5) });
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        const string original = """{"event":{"id":"ev_test"}}""";
        var payload = variant switch { "malformed" => "{", "no-event" => "{}", _ => original };
        using var post = SignedPost(payload, variant == "stale" ? DateTimeOffset.UtcNow.AddHours(-1) : null);
        if (variant == "missing") post.Headers.Remove(CloseWebhook.SignatureHeader);
        if (variant == "duplicate") post.Headers.Add(CloseWebhook.SignatureHeader, new string('f', 64));
        if (variant == "changed")
        {
            post.Content!.Dispose();
            post.Content = new StringContent(original + " ");
        }

        using var response = await http.SendAsync(post, token);
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(0, Volatile.Read(ref calls));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BodyLimitRejectsOversizedRequestsWithOrWithoutContentLength(bool chunked)
    {
        var token = TestContext.Current.CancellationToken;
        var calls = 0;
        await using var app = await StartAsync((_, _) =>
        {
            Interlocked.Increment(ref calls);
            return Task.CompletedTask;
        }, token, new() { MaxBodyBytes = 40 });
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        var payload = """{"event":{"id":"ev_test"}}""" + new string(' ', 100);
        using var post = SignedPost(payload);
        if (chunked)
        {
            post.Content!.Dispose();
            post.Content = new StreamContent(new NonSeekableBody(Encoding.UTF8.GetBytes(payload)));
            post.Headers.TransferEncodingChunked = true;
        }

        using var response = await http.SendAsync(post, token);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(0, Volatile.Read(ref calls));
    }

    [Fact]
    public async Task HandlerFailureReturnsAnErrorSoCloseCanRetry()
    {
        var token = TestContext.Current.CancellationToken;
        await using var app = await StartAsync((_, _) => throw new InvalidOperationException("Queue unavailable."), token);
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        using var post = SignedPost("""{"event":{"id":"ev_retry"}}""");
        using var response = await http.SendAsync(post, token);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private static async Task<WebApplication> StartAsync(
        Func<CloseWebhookRequest, CancellationToken, Task> handler, CancellationToken token,
        CloseWebhookEndpointOptions? options = null)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        app.MapCloseWebhook("/hooks/close", Key, handler, options);
        await app.StartAsync(token);
        return app;
    }

    private static HttpRequestMessage SignedPost(string payload, DateTimeOffset? now = null)
    {
        var timestamp = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        var signature = Convert.ToHexString(HMACSHA256.HashData(Convert.FromHexString(Key), Encoding.UTF8.GetBytes(timestamp + payload)));
        var post = new HttpRequestMessage(HttpMethod.Post, "/hooks/close")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        post.Headers.Add(CloseWebhook.TimestampHeader, timestamp);
        post.Headers.Add(CloseWebhook.SignatureHeader, signature);
        return post;
    }

    private sealed class NonSeekableBody(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
    }
}
