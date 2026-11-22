using System.Text.Json;

namespace Close.DotNet;

internal static class EndpointTransport
{
    internal static byte[] Serialize<T>(T request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return JsonSerializer.SerializeToUtf8Bytes(request, EndpointJson.Type<T>());
    }

    internal static async Task<T> SendAsync<T>(CloseTransport transport, HttpMethod method,
        string path, Dictionary<string, string?>? query, byte[]? body, CancellationToken cancellationToken)
    {
        var reply = await transport.SendAsync(method, path, query, body, EndpointJson.Type<T>(),
            method == HttpMethod.Get, cancellationToken).ConfigureAwait(false);
        if (reply.Data is not { } data)
            throw new CloseApiException(reply.StatusCode, reply.RequestId, reply.RateLimit,
                responseProblem: "Close returned an empty response where JSON was expected.");
        return data;
    }

    internal static async Task SendWithoutBodyAsync(CloseTransport transport, HttpMethod method,
        string path, Dictionary<string, string?>? query, byte[]? body, CancellationToken cancellationToken)
    {
        await transport.SendAsync<JsonElement>(method, path, query, body, null,
            method == HttpMethod.Get, cancellationToken).ConfigureAwait(false);
    }

    internal static Task<ClosePage<T>> ListPageAsync<T>(CloseTransport transport, string path,
        CloseListOptions? options, CancellationToken token)
    {
        options ??= new();
        return ReadPageAsync<T>(transport, path, options.ToQuery(), options.Skip, options.Limit, token);
    }

    internal static IAsyncEnumerable<T> StreamAsync<T>(CloseTransport transport, string path,
        CloseListOptions? options, CancellationToken token) =>
        CloseRoutes.StreamOffsetAsync<T>(options ?? new(),
            (query, skip, limit, cancellationToken) => ReadPageAsync<T>(transport, path, query, skip, limit, cancellationToken), token);

    private static async Task<ClosePage<T>> ReadPageAsync<T>(CloseTransport transport, string path,
        Dictionary<string, string?> query, int skip, int? limit, CancellationToken token)
    {
        CloseRoutes.SetOffset(query, skip, limit);
        var page = await SendAsync<ClosePage<T>>(transport, HttpMethod.Get, path, query, null, token).ConfigureAwait(false);
        return page.Data is not null ? page : throw new InvalidOperationException("Close returned a null list.");
    }

    internal static Dictionary<string, string?> RequiredQuery(CloseRequestOptions? options, params (string Key, string Value)[] required)
    {
        var query = options?.ToQuery() ?? [];
        foreach (var (key, value) in required)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, key);
            query[key] = value;
        }
        return query;
    }
}
