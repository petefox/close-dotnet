using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Close.DotNet;

internal sealed class CloseTransport(HttpClient httpClient, CloseCredential credential, CloseClientOptions options)
{
    private const int MaxErrorBytes = 16 * 1024;
    private readonly Uri baseAddress = ValidateOptions(options);

    internal async Task<CloseResponse<T?>> SendAsync<T>(
        HttpMethod method, string path, IReadOnlyDictionary<string, string?>? query, byte[]? body,
        JsonTypeInfo<T>? responseType, bool retry, CancellationToken cancellationToken)
    {
        var uri = RequestUri(path, query);
        var remainingDelay = options.RetryDelayBudget;
        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Accept.Add(new("application/json"));
            request.Headers.UserAgent.ParseAdd("Close.DotNet/" + VersionInfo.Value);
            request.Headers.Authorization = await credential.GetHeaderAsync(cancellationToken).ConfigureAwait(false);
            if (body is not null)
            {
                request.Content = new ByteArrayContent(body);
                request.Content.Headers.ContentType = new("application/json");
            }

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            TimeSpan delay;
            using (response)
            {
                var rate = ReadRateLimit(response);
                var requestId = Header(response, "X-Request-Id") ?? Header(response, "Request-Id");
                if (response.IsSuccessStatusCode)
                {
                    return await ReadSuccessAsync(response, responseType, requestId, rate, cancellationToken).ConfigureAwait(false);
                }

                var details = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.TooManyRequests && rate?.ResetAfter is null &&
                    details is { ValueKind: JsonValueKind.Object } error && error.TryGetProperty("rate_reset", out var reset) &&
                    reset.TryGetDoubleOrString(out var resetSeconds) && Seconds(resetSeconds) is { } legacyDelay)
                {
                    rate = new(rate?.Limit, rate?.Remaining, legacyDelay, rate?.RetryAfter);
                }

                delay = rate?.ResetAfter ?? rate?.RetryAfter ?? TimeSpan.FromMilliseconds(250 * (1 << Math.Min(attempt, 8)));
                if (!retry || attempt >= options.MaxReadRetries || delay > remainingDelay ||
                    response.StatusCode is not (HttpStatusCode.TooManyRequests or HttpStatusCode.BadGateway
                        or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout))
                    throw new CloseApiException(response.StatusCode, requestId, rate, details);
            }

            remainingDelay -= delay;
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    internal static async Task<JsonElement?> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var buffer = new byte[MaxErrorBytes + 1];
        var count = await stream.ReadAtLeastAsync(buffer, buffer.Length, false, cancellationToken)
            .ConfigureAwait(false);
        if (count == 0 || count > MaxErrorBytes) return null;
        try
        {
            using var document = JsonDocument.Parse(buffer.AsMemory(0, count));
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static async Task<CloseResponse<T?>> ReadSuccessAsync<T>(
        HttpResponseMessage response, JsonTypeInfo<T>? type, string? requestId, CloseRateLimit? rate, CancellationToken token)
    {
        if (type is null || response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            return new(default, response.StatusCode, requestId, rate);

        using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        try
        {
            var data = await JsonSerializer.DeserializeAsync(stream, type, token).ConfigureAwait(false);
            return new(data, response.StatusCode, requestId, rate);
        }
        catch (JsonException)
        {
            throw new CloseApiException(response.StatusCode, requestId, rate, responseProblem: "Close returned an invalid JSON response.");
        }
    }

    private Uri RequestUri(string path, IReadOnlyDictionary<string, string?>? query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (path.StartsWith('/') || path.Contains(':') || path.Contains('?') || path.Contains('#') ||
            path.Contains('\\') || path.Any(char.IsControl) ||
            path.Split('/').Any(segment =>
            {
                var decoded = Uri.UnescapeDataString(segment);
                return decoded is "." or ".." || decoded.IndexOfAny(['/', '\\', '%']) >= 0;
            }))
            throw new ArgumentException("Use a relative API path such as lead/ and supply query parameters separately.", nameof(path));

        var uri = new Uri(baseAddress, path);
        if (!baseAddress.IsBaseOf(uri))
            throw new ArgumentException("The request must stay within the Close API base address.", nameof(path));
        if (query is null || query.Count == 0) return uri;
        var builder = new UriBuilder(uri)
        {
            Query = string.Join('&', query.Where(entry => entry.Value is not null).Select(entry =>
                $"{Uri.EscapeDataString(entry.Key)}={Uri.EscapeDataString(entry.Value!)}"))
        };
        return builder.Uri;
    }

    private static Uri ValidateOptions(CloseClientOptions value)
    {
        ArgumentNullException.ThrowIfNull(value.BaseAddress);
        var address = value.BaseAddress;
        if (!address.IsAbsoluteUri ||
            (address.Scheme != Uri.UriSchemeHttps && !(address.Scheme == Uri.UriSchemeHttp && address.IsLoopback)) ||
            address.UserInfo.Length > 0 || address.Query.Length > 0 || address.Fragment.Length > 0)
            throw new ArgumentException("Use an absolute HTTPS base address, or HTTP for loopback testing.", nameof(value));

        ArgumentOutOfRangeException.ThrowIfNegative(value.MaxReadRetries);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.MaxReadRetries, 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(value.RetryDelayBudget, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.RetryDelayBudget, TimeSpan.FromMinutes(5));
        return address.AbsoluteUri.EndsWith('/') ? address : new Uri(address.AbsoluteUri + "/");
    }

    private static CloseRateLimit? ReadRateLimit(HttpResponseMessage response)
    {
        int? limit = null, remaining = null;
        TimeSpan? reset = null;
        if (Header(response, "RateLimit") is { } header)
        {
            foreach (var part in header.Split(','))
            {
                var pair = part.Split('=', 2, StringSplitOptions.TrimEntries);
                if (pair.Length != 2) continue;
                if (pair[0].Equals("limit", StringComparison.OrdinalIgnoreCase))
                    limit = Integer(pair[1]);
                if (pair[0].Equals("remaining", StringComparison.OrdinalIgnoreCase))
                    remaining = Integer(pair[1]);
                if (pair[0].Equals("reset", StringComparison.OrdinalIgnoreCase) &&
                    double.TryParse(pair[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
                    reset = Seconds(seconds);
            }
        }

        var retry = response.Headers.RetryAfter;
        var after = retry?.Delta ?? (retry?.Date is { } date ? date - DateTimeOffset.UtcNow : null);
        if (after < TimeSpan.Zero) after = TimeSpan.Zero;
        return limit is null && remaining is null && reset is null && after is null ? null : new(limit, remaining, reset, after);
    }

    private static int? Integer(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) ? result : null;

    private static TimeSpan? Seconds(double seconds) =>
        double.IsFinite(seconds) && seconds >= 0 && seconds < TimeSpan.MaxValue.TotalSeconds
            ? TimeSpan.FromSeconds(seconds) : null;

    internal static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
}

internal static class JsonElementNumbers
{
    internal static bool TryGetDoubleOrString(this JsonElement value, out double number)
    {
        number = 0;
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetDouble(out number),
            JsonValueKind.String => double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number),
            _ => false
        };
    }
}

internal static class VersionInfo
{
    internal static string Value { get; } =
        typeof(CloseClient).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
}
