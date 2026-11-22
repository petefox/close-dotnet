using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Close.DotNet;

/// <summary>Filter for a webhook event subscription.</summary>
/// <param name="ObjectType">Close object type, such as lead or activity.note.</param>
/// <param name="Action">Close event action, such as created or updated.</param>
/// <param name="ExtraFilter">Optional Close filter predicate.</param>
public sealed record WebhookEventFilter(string ObjectType, string Action, JsonElement? ExtraFilter = null);

/// <summary>The event in a verified Close webhook delivery.</summary>
public sealed class CloseWebhookDelivery
{
    /// <summary>The event attached to the delivery.</summary>
    public CloseEvent? Event { get; init; }
}

/// <summary>Verifies and reads webhook deliveries from Close.</summary>
public static class CloseWebhook
{
    /// <summary>Name of the header containing the hex-encoded signature.</summary>
    public const string SignatureHeader = "close-sig-hash";

    /// <summary>Name of the header containing the signing timestamp.</summary>
    public const string TimestampHeader = "close-sig-timestamp";

    /// <summary>Verifies the exact raw body signed by Close. Optionally rejects timestamps outside a symmetric freshness window.</summary>
    public static bool VerifySignature(
        ReadOnlySpan<byte> body, string? timestamp, string? signature, string signingKey,
        TimeSpan? maxAge = null, TimeProvider? timeProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signingKey);
        if (maxAge is { } age) ArgumentOutOfRangeException.ThrowIfLessThan(age, TimeSpan.Zero);
        if (timestamp is null || timestamp.Length > 19 || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) ||
            seconds > DateTimeOffset.MaxValue.ToUnixTimeSeconds() || signature is null || signature.Length != 64)
            return false;
        if (maxAge is { } window &&
            ((timeProvider ?? TimeProvider.System).GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(seconds)).Duration() > window)
            return false;

        Span<byte> expected = stackalloc byte[32];
        if (Convert.FromHexString(signature, expected, out _, out _) != System.Buffers.OperationStatus.Done) return false;

        if (signingKey.Length % 2 != 0 || !signingKey.All(Uri.IsHexDigit))
            throw new ArgumentException("Close signing keys must be hexadecimal.", nameof(signingKey));
        var key = Convert.FromHexString(signingKey);
        try
        {
            using var hmac = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key);
            Span<byte> timestampBytes = stackalloc byte[32];
            var size = Encoding.ASCII.GetBytes(timestamp.AsSpan(), timestampBytes);
            hmac.AppendData(timestampBytes[..size]);
            hmac.AppendData(body);
            Span<byte> actual = stackalloc byte[32];
            hmac.GetHashAndReset(actual);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
        }
    }

    /// <summary>Deserializes a verified delivery body.</summary>
    public static CloseWebhookDelivery ReadDelivery(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize(body, CloseJson.Type<CloseWebhookDelivery>()) ??
        throw new JsonException("Expected a Close webhook delivery.");
}
