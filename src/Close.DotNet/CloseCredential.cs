using System.Net.Http.Headers;
using System.Text;

namespace Close.DotNet;

/// <summary>Authentication for a single Close client or connection.</summary>
public sealed class CloseCredential
{
    private readonly Func<CancellationToken, ValueTask<AuthenticationHeaderValue>> getHeader;

    private CloseCredential(Func<CancellationToken, ValueTask<AuthenticationHeaderValue>> getHeader) =>
        this.getHeader = getHeader;

    /// <summary>Uses HTTP Basic authentication with the API key as the username.</summary>
    public static CloseCredential ApiKey(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        if (apiKey.Contains(':') || apiKey.Any(char.IsControl))
            throw new ArgumentException("The API key contains an invalid character.", nameof(apiKey));
        var value = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":")));
        return new(_ => ValueTask.FromResult(value));
    }

    /// <summary>Uses a fixed OAuth access token.</summary>
    public static CloseCredential AccessToken(string accessToken)
    {
        var value = Bearer(accessToken);
        return new(_ => ValueTask.FromResult(value));
    }

    /// <summary>Obtains the current OAuth access token from the caller before each attempt.</summary>
    public static CloseCredential AccessTokenProvider(Func<CancellationToken, ValueTask<string>> provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return new(async cancellationToken => Bearer(await provider(cancellationToken).ConfigureAwait(false)));
    }

    private static AuthenticationHeaderValue Bearer(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)))
            throw new ArgumentException("The OAuth token contains an invalid character.", nameof(value));
        return new("Bearer", value);
    }

    internal ValueTask<AuthenticationHeaderValue> GetHeaderAsync(CancellationToken cancellationToken) =>
        getHeader(cancellationToken);
}
