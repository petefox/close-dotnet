using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Close.DotNet.Tests;

public sealed class TransportTests
{
    [Fact]
    public async Task SharedHttpClientSendsCredentialsPerRequestAndDoesNotMutateDefaults()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"id":"user_demo"}"""));
        using var http = new HttpClient(handler);
        var api = TestClient.Create(http);
        var first = new CloseClient(CloseCredential.AccessToken("token_a"), http);
        var second = new CloseClient(CloseCredential.AccessTokenProvider(_ => ValueTask.FromResult("token_b")), http);

        await Task.WhenAll(api.Users.GetMeAsync(token), first.Users.GetMeAsync(token), second.Users.GetMeAsync(token));

        Assert.Equal(new[] { "Basic dGVzdF9rZXk6", "Bearer token_a", "Bearer token_b" },
            handler.Requests.Select(request => request.Authorization).Order(StringComparer.Ordinal));
        Assert.Null(http.DefaultRequestHeaders.Authorization);
    }

    [Fact]
    public async Task DependencyInjectionResolvesCredentialFromTheCurrentScope()
    {
        var token = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddScoped<ScopedCredential>();
        var captures = new System.Collections.Concurrent.ConcurrentQueue<string?>();
        services.AddCloseClient(provider => CloseCredential.AccessToken(provider.GetRequiredService<ScopedCredential>().Token))
            .ConfigurePrimaryHttpMessageHandler(() => new ScriptedHandler(request =>
            {
                captures.Enqueue(request.Authorization);
                return JsonResponse.Ok("""{"id":"user_demo"}""");
            }));
        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        var firstExpected = first.ServiceProvider.GetRequiredService<ScopedCredential>().Token;
        var secondExpected = second.ServiceProvider.GetRequiredService<ScopedCredential>().Token;
        await first.ServiceProvider.GetRequiredService<CloseClient>().Users.GetMeAsync(token);
        await second.ServiceProvider.GetRequiredService<CloseClient>().Users.GetMeAsync(token);
        Assert.Equal(new[] { "Bearer " + firstExpected, "Bearer " + secondExpected }, captures);
        Assert.NotEqual(firstExpected, secondExpected);
    }

    [Fact]
    public async Task RawMethodsPreserveJsonAndExposeResponseMetadata()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ =>
        {
            var response = JsonResponse.Ok("""{"name":"Other"}""");
            response.Headers.Add("X-Request-Id", "req_77");
            response.Headers.Add("RateLimit", "limit=40, remaining=33, reset=0.625");
            return response;
        });
        using var http = new HttpClient(handler);
        using var document = JsonDocument.Parse("""{"data":[1,false,null]}""");
        var result = await TestClient.Create(http).SendAsync(HttpMethod.Post, "other/", new Dictionary<string, string?>
        {
            ["filter"] = "a#b", ["omit"] = null
        }, document.RootElement, token);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("/api/v1/other/", request.Uri.AbsolutePath);
        Assert.Equal("?filter=a%23b", request.Uri.Query);
        Assert.Equal(document.RootElement.GetRawText(), request.Body);
        Assert.Equal("Other", result.Data.GetProperty("name").GetString());
        Assert.Equal("req_77", result.RequestId);
        Assert.Equal(new CloseRateLimit(40, 33, TimeSpan.FromMilliseconds(625), null), result.RateLimit);
    }

    [Theory]
    [InlineData("https://example.org/elsewhere/")]
    [InlineData("//example.org/elsewhere/")]
    [InlineData("../outside/")]
    [InlineData("%2e%2e/outside/")]
    [InlineData("%252e%252e/outside/")]
    [InlineData("lead%2Foutside/")]
    [InlineData("lead/?query=bad")]
    public async Task RawMethodRejectsAbsoluteOrEscapingPaths(string invalidPath)
    {
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("{}"));
        using var http = new HttpClient(handler);
        var api = TestClient.Create(http);
        await Assert.ThrowsAsync<ArgumentException>(() => api.SendAsync(HttpMethod.Get, invalidPath,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ReadsRetryAtMostTwiceAndHonorTheServerResetBudget()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Throttled("0"));
        using var http = new HttpClient(handler);
        var api = TestClient.Create(http);

        var final = await Assert.ThrowsAsync<CloseApiException>(() => api.Users.GetMeAsync(token));
        Assert.Equal(HttpStatusCode.TooManyRequests, final.StatusCode);
        Assert.Equal(3, handler.Requests.Count);

        using var boundedHandler = new ScriptedHandler(_ => JsonResponse.Throttled("0.875"));
        using var boundedHttp = new HttpClient(boundedHandler);
        var bounded = TestClient.Create(boundedHttp, new() { RetryDelayBudget = TimeSpan.FromMilliseconds(500) });
        var error = await Assert.ThrowsAsync<CloseApiException>(() => bounded.Users.GetMeAsync(token));
        Assert.Equal(TimeSpan.FromMilliseconds(875), error.RateLimit?.ResetAfter);
        Assert.Equal(TimeSpan.Zero, error.RateLimit?.RetryAfter);
        Assert.Single(boundedHandler.Requests);
    }

    [Fact]
    public async Task SearchReplaysSafePostButNeverRetriesAWrite()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 2
            ? JsonResponse.Ok("""{"data":[]}""") : JsonResponse.Throttled("0"));
        using var http = new HttpClient(handler);
        var close = TestClient.Create(http);

        await close.Search.SearchPageAsync(new() { Limit = 5 }, token);
        var error = await Assert.ThrowsAsync<CloseApiException>(() => close.Leads.CreateAsync(new() { Name = "Sample" }, cancellationToken: token));

        var requests = handler.Requests.ToArray();
        Assert.Equal(3, requests.Length);
        Assert.Equal(requests[0].Body, requests[1].Body);
        Assert.Equal("/api/v1/lead/", requests[2].Uri.AbsolutePath);
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
    }

    [Fact]
    public async Task CancellingRateLimitWaitDoesNotReplayTheRequest()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new ScriptedHandler(_ =>
        {
            cancellation.CancelAfter(TimeSpan.FromMilliseconds(80));
            return JsonResponse.Throttled("8");
        });
        using var http = new HttpClient(handler);
        var close = TestClient.Create(http);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => close.Users.GetMeAsync(cancellation.Token));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task ErrorExposesStructuredDetailsWithoutEchoingTheBodyInItsMessage()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ =>
        {
            var response = JsonResponse.Ok("""{"field-errors":{"name":"sensitive example"}}""", HttpStatusCode.BadRequest);
            response.Headers.Add("X-Request-Id", "req_case");
            return response;
        });
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<CloseApiException>(() => TestClient.Create(http).Users.GetMeAsync(token));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Equal("req_case", error.RequestId);
        Assert.Equal("sensitive example", error.Details?.GetProperty("field-errors").GetProperty("name").GetString());
        Assert.DoesNotContain("sensitive example", error.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task StandaloneHttpClientSendsApiKeyAndDoesNotFollowRedirects()
    {
        var token = TestContext.Current.CancellationToken;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var receive = WriteRedirectAsync();
        var sdk = new CloseClient("test_key", new() { BaseAddress = new($"http://127.0.0.1:{port}/api/v1/") });

        var error = await Assert.ThrowsAsync<CloseApiException>(() => sdk.Users.GetMeAsync(token));
        var headers = await receive;
        Assert.Equal(HttpStatusCode.Redirect, error.StatusCode);
        Assert.Contains("GET /api/v1/me/ HTTP/1.1", headers);
        Assert.Contains("Authorization: Basic dGVzdF9rZXk6", headers, StringComparer.OrdinalIgnoreCase);
        Assert.False(listener.Pending());

        async Task<List<string>> WriteRedirectAsync()
        {
            using var connection = await listener.AcceptTcpClientAsync(token);
            using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            var lines = new List<string>();
            while (await reader.ReadLineAsync(token) is { Length: > 0 } line) lines.Add(line);
            await stream.WriteAsync(Encoding.ASCII.GetBytes(
                $"HTTP/1.1 302 Found\r\nLocation: http://127.0.0.1:{port}/should-not-follow\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"), token);
            return lines;
        }
    }

    private sealed class ScopedCredential
    {
        public string Token { get; } = Guid.NewGuid().ToString("N");
    }
}
