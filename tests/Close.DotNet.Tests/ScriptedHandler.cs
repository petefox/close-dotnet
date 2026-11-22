using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Close.DotNet.Tests;

internal sealed record CapturedRequest(HttpMethod Method, Uri Uri, string? Authorization, string? MediaType, string? Body, int Attempt);

internal sealed class ScriptedHandler(Func<CapturedRequest, HttpResponseMessage> reply) : HttpMessageHandler
{
    public ConcurrentQueue<CapturedRequest> Requests { get; } = new();
    private int count;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var captured = new CapturedRequest(request.Method, request.RequestUri!, request.Headers.Authorization?.ToString(),
            request.Content?.Headers.ContentType?.MediaType,
            request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken),
            Interlocked.Increment(ref count));
        Requests.Enqueue(captured);
        return reply(captured);
    }
}

internal static class JsonResponse
{
    internal static HttpResponseMessage Ok(string value, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(value, Encoding.UTF8, "application/json") };

    internal static HttpResponseMessage Throttled(string reset)
    {
        var result = Ok("""{"error":"slow down"}""", HttpStatusCode.TooManyRequests);
        result.Headers.TryAddWithoutValidation("RateLimit", "limit=20, remaining=0, reset=" + reset);
        result.Headers.TryAddWithoutValidation("Retry-After", "0");
        return result;
    }
}

internal static class TestClient
{
    internal static CloseClient Create(HttpClient http, CloseClientOptions? options = null) =>
        new(CloseCredential.ApiKey("test_key"), http, options);
}
