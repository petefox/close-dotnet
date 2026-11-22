using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Close.DotNet.AspNetCore;

/// <summary>Settings applied to an ASP.NET Core Close webhook route.</summary>
public sealed class CloseWebhookEndpointOptions
{
    /// <summary>Maximum accepted body size in bytes. Defaults to 1 MiB.</summary>
    public int MaxBodyBytes { get; init; } = 1024 * 1024;

    /// <summary>Optional freshness window for timestamps. Leave unset to accept delayed deliveries.</summary>
    public TimeSpan? SignatureMaxAge { get; init; }
}

/// <summary>A verified Close event and its original request.</summary>
public sealed class CloseWebhookRequest
{
    internal CloseWebhookRequest(CloseEvent change, ReadOnlyMemory<byte> rawBody, HttpContext context)
    {
        Event = change;
        RawBody = rawBody;
        Context = context;
    }

    /// <summary>The verified event with a Close event ID.</summary>
    public CloseEvent Event { get; }

    /// <summary>The original bytes covered by the Close signature.</summary>
    public ReadOnlyMemory<byte> RawBody { get; }

    /// <summary>Request context, including access to request-scoped services.</summary>
    public HttpContext Context { get; }
}

/// <summary>Extension methods that map verified Close webhook POST routes.</summary>
public static class CloseWebhookEndpointExtensions
{
    /// <summary>Maps one POST route for a Close subscription with a fixed hexadecimal signing key.</summary>
    /// <remarks>The route returns HTTP 204 after the handler succeeds. Persist or enqueue the event in the handler before returning.</remarks>
    public static RouteHandlerBuilder MapCloseWebhook(this IEndpointRouteBuilder endpoints, string pattern,
        string signingKey, Func<CloseWebhookRequest, CancellationToken, Task> handler,
        CloseWebhookEndpointOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signingKey);
        if (signingKey.Length % 2 != 0 || !signingKey.All(Uri.IsHexDigit))
            throw new ArgumentException("Close signing keys must be hexadecimal.", nameof(signingKey));
        return endpoints.MapCloseWebhook(pattern, (_, _) => ValueTask.FromResult<string?>(signingKey), handler, options);
    }

    /// <summary>Maps a Close webhook POST route that resolves its signing key from request-scoped services.</summary>
    /// <remarks>Resolve the key from trusted route or application configuration. A null key rejects the request.</remarks>
    public static RouteHandlerBuilder MapCloseWebhook(this IEndpointRouteBuilder endpoints, string pattern,
        Func<HttpContext, CancellationToken, ValueTask<string?>> resolveSigningKey,
        Func<CloseWebhookRequest, CancellationToken, Task> handler, CloseWebhookEndpointOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        ArgumentNullException.ThrowIfNull(resolveSigningKey);
        ArgumentNullException.ThrowIfNull(handler);
        var settings = options ?? new CloseWebhookEndpointOptions();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(settings.MaxBodyBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(settings.MaxBodyBytes, Array.MaxLength - 1);
        if (settings.SignatureMaxAge is { } age) ArgumentOutOfRangeException.ThrowIfLessThan(age, TimeSpan.Zero);

        return endpoints.MapPost(pattern, async (HttpContext context, CancellationToken token) =>
        {
            var request = context.Request;
            if (request.ContentLength > settings.MaxBodyBytes)
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

            var signatures = request.Headers[CloseWebhook.SignatureHeader];
            var timestamps = request.Headers[CloseWebhook.TimestampHeader];
            if (signatures.Count != 1 || timestamps.Count != 1 || signatures[0]?.Length != 64 ||
                timestamps[0] is not { Length: >= 1 and <= 19 })
                return Results.Unauthorized();

            var key = await resolveSigningKey(context, token);
            if (string.IsNullOrWhiteSpace(key)) return Results.Unauthorized();

            // Read one extra byte to identify oversized requests that have no Content-Length header.
            var capacity = request.ContentLength is { } length
                ? checked((int)length + 1)
                : settings.MaxBodyBytes + 1;
            var buffer = ArrayPool<byte>.Shared.Rent(capacity);
            CloseWebhookRequest delivery;
            try
            {
                var read = await request.Body.ReadAtLeastAsync(buffer.AsMemory(0, capacity), capacity, false, token);
                if (read > settings.MaxBodyBytes)
                    return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

                if (!CloseWebhook.VerifySignature(buffer.AsSpan(0, read), timestamps[0], signatures[0], key, settings.SignatureMaxAge))
                    return Results.Unauthorized();

                CloseEvent? change;
                try
                {
                    change = CloseWebhook.ReadDelivery(buffer.AsSpan(0, read)).Event;
                }
                catch (JsonException)
                {
                    return Results.BadRequest();
                }
                if (string.IsNullOrWhiteSpace(change?.Id)) return Results.BadRequest();

                delivery = new CloseWebhookRequest(change, buffer.AsMemory(0, read).ToArray(), context);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(buffer.AsSpan(0, capacity));
                ArrayPool<byte>.Shared.Return(buffer);
            }

            await handler(delivery, token);
            return Results.NoContent();
        });
    }
}
