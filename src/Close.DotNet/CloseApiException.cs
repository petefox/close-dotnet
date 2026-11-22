using System.Net;
using System.Text.Json;

namespace Close.DotNet;

/// <summary>A failed API request or an invalid API response.</summary>
public sealed class CloseApiException : HttpRequestException
{
    internal CloseApiException(
        HttpStatusCode statusCode, string? requestId, CloseRateLimit? rateLimit,
        JsonElement? details = null, string? responseProblem = null, Exception? innerException = null)
        : base(responseProblem ?? $"Close returned HTTP {(int)statusCode} ({statusCode}).", innerException, statusCode)
    {
        RequestId = requestId;
        RateLimit = rateLimit;
        Details = details;
    }

    /// <summary>Request identifier returned by Close, when available.</summary>
    public string? RequestId { get; }

    /// <summary>Rate-limit or retry timing reported by Close.</summary>
    public CloseRateLimit? RateLimit { get; }

    /// <summary>Bounded JSON error details, which may contain API data. Inspect before logging.</summary>
    public JsonElement? Details { get; }
}

/// <summary>Rate-limit information sent by Close.</summary>
/// <param name="Limit">Request limit for the relevant group.</param>
/// <param name="Remaining">Requests remaining in the current enforcement window.</param>
/// <param name="ResetAfter">Precise delay from the RateLimit header or legacy error body.</param>
/// <param name="RetryAfter">Delay reported by the Retry-After header.</param>
public sealed record CloseRateLimit(int? Limit, int? Remaining, TimeSpan? ResetAfter, TimeSpan? RetryAfter);

/// <summary>Response data and HTTP metadata for a raw API call.</summary>
/// <typeparam name="T">Response body type.</typeparam>
/// <param name="Data">Response body, if present.</param>
/// <param name="StatusCode">HTTP status code.</param>
/// <param name="RequestId">Request identifier returned by Close.</param>
/// <param name="RateLimit">Rate-limit information, if present.</param>
public sealed record CloseResponse<T>(T Data, HttpStatusCode StatusCode, string? RequestId, CloseRateLimit? RateLimit);
