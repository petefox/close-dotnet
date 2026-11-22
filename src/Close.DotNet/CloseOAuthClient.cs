using System.Net.Http.Headers;
using System.Text.Json;

namespace Close.DotNet;

/// <summary>Performs Close OAuth authorization-code, refresh, and revocation operations.</summary>
public sealed class CloseOAuthClient
{
    private static readonly HttpClient SharedHttpClient = new(CloseClient.CreateHandler());
    private static readonly Uri AuthorizeUri = new("https://app.close.com/oauth2/authorize/");
    private static readonly Uri TokenUri = new("https://api.close.com/oauth2/token/");
    private static readonly Uri RevokeUri = new("https://api.close.com/oauth2/revoke/");
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly Uri redirectUri;
    private readonly HttpClient httpClient;

    /// <summary>Creates an OAuth client for a registered Close app and its exact HTTPS redirect URI.</summary>
    /// <remarks>If supplying transport, disable redirects and keep that client dedicated to the OAuth server.</remarks>
    public CloseOAuthClient(string clientId, string clientSecret, Uri redirectUri, HttpClient? httpClient = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        ArgumentNullException.ThrowIfNull(redirectUri);
        if (!redirectUri.IsAbsoluteUri || redirectUri.Scheme != Uri.UriSchemeHttps ||
            redirectUri.UserInfo.Length != 0 || redirectUri.Fragment.Length != 0)
            throw new ArgumentException("The redirect URI must be an absolute HTTPS URL without a fragment.", nameof(redirectUri));
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.redirectUri = redirectUri;
        this.httpClient = httpClient ?? SharedHttpClient;
    }

    /// <summary>Creates a consent URL. Generate and verify a single-use state value in the application.</summary>
    public Uri CreateAuthorizationUri(string state, IEnumerable<string>? scopes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        var query = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri.AbsoluteUri,
            ["response_type"] = "code",
            ["state"] = state
        };
        if (scopes is not null) query["scope"] = string.Join(' ', scopes);
        return new UriBuilder(AuthorizeUri)
        {
            Query = string.Join('&', query.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"))
        }.Uri;
    }

    /// <summary>Exchanges a validated authorization code for tokens.</summary>
    public Task<CloseOAuthTokens> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return GetTokensAsync(new()
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri.AbsoluteUri
        }, cancellationToken);
    }

    /// <summary>Exchanges a refresh token exactly once. Persist the replacement token returned by Close.</summary>
    public Task<CloseOAuthTokens> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        return GetTokensAsync(new() { ["grant_type"] = "refresh_token", ["refresh_token"] = refreshToken }, cancellationToken);
    }

    /// <summary>Revokes access using the refresh token for the connection.</summary>
    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        using var response = await PostAsync(RevokeUri, new() { ["token"] = refreshToken }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<CloseOAuthTokens> GetTokensAsync(Dictionary<string, string> fields, CancellationToken token)
    {
        using var response = await PostAsync(TokenUri, fields, token).ConfigureAwait(false);
        var requestId = CloseTransport.Header(response, "X-Request-Id");
        using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        CloseOAuthTokens? data;
        try
        {
            data = await JsonSerializer.DeserializeAsync(stream, CloseJson.Type<CloseOAuthTokens>(), token).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            throw new CloseApiException(response.StatusCode, requestId, null, responseProblem: "Close returned an invalid token response.");
        }
        if (data is null || string.IsNullOrWhiteSpace(data.AccessToken) || data.ExpiresIn <= 0)
            throw new CloseApiException(response.StatusCode, requestId, null, responseProblem: "Close returned an incomplete token response.");
        return data;
    }

    private async Task<HttpResponseMessage> PostAsync(Uri url, Dictionary<string, string> fields, CancellationToken token)
    {
        fields["client_id"] = clientId;
        fields["client_secret"] = clientSecret;
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
        if (response.IsSuccessStatusCode) return response;
        using (response)
        {
            var details = await CloseTransport.ReadErrorAsync(response, token).ConfigureAwait(false);
            throw new CloseApiException(response.StatusCode, CloseTransport.Header(response, "X-Request-Id"), null, details);
        }
    }
}

/// <summary>OAuth tokens and connection metadata. Store tokens as secrets.</summary>
public sealed class CloseOAuthTokens
{
    /// <summary>Token for authenticating API requests.</summary>
    public string AccessToken { get; init; } = "";

    /// <summary>Replacement token for generating a new access token.</summary>
    public string? RefreshToken { get; init; }

    /// <summary>Lifetime of the access token in seconds.</summary>
    public int ExpiresIn { get; init; }

    /// <summary>OAuth authentication scheme.</summary>
    public string? TokenType { get; init; }

    /// <summary>Space-separated granted OAuth scopes.</summary>
    public string? Scope { get; init; }

    /// <summary>Close organization selected during consent.</summary>
    public string? OrganizationId { get; init; }

    /// <summary>Close user who authorized the connection.</summary>
    public string? UserId { get; init; }
}
