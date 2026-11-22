using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Close.DotNet.Tests;

public sealed class OAuthAndWebhookTests
{
    [Fact]
    public async Task OAuthBuildsConsentUrlAndUsesFormEncodedGrantRefreshAndRevoke()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt switch
        {
            1 => JsonResponse.Ok("""{"access_token":"access_1","expires_in":3600,"refresh_token":"refresh_1","organization_id":"orga_demo"}"""),
            2 => JsonResponse.Ok("""{"access_token":"access_2","expires_in":3600,"refresh_token":"refresh_2"}"""),
            _ => new HttpResponseMessage(HttpStatusCode.NoContent)
        });
        using var http = new HttpClient(handler);
        var oauth = new CloseOAuthClient("client_id", "secret+&", new("https://example.org/callback/close"), http);

        var consent = oauth.CreateAuthorizationUri("match + this", ["all.read", "offline_access"]);
        var first = await oauth.ExchangeCodeAsync("code+a", token);
        var refreshed = await oauth.RefreshAsync(first.RefreshToken!, token);
        await oauth.RevokeAsync(refreshed.RefreshToken!, token);

        Assert.Equal("app.close.com", consent.Host);
        Assert.Equal("/oauth2/authorize/", consent.AbsolutePath);
        Assert.Contains("response_type=code", consent.Query);
        Assert.Contains("state=match%20%2B%20this", consent.Query);
        Assert.Contains("scope=all.read%20offline_access", consent.Query);
        Assert.Contains("redirect_uri=https%3A%2F%2Fexample.org%2Fcallback%2Fclose", consent.Query);
        Assert.DoesNotContain("secret", consent.Query);
        Assert.Equal("orga_demo", first.OrganizationId);
        Assert.Equal("access_2", refreshed.AccessToken);
        Assert.Equal("refresh_2", refreshed.RefreshToken);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("api.close.com", request.Uri.Host);
            Assert.Equal("application/x-www-form-urlencoded", request.MediaType);
            Assert.Null(request.Authorization);
            Assert.Contains("client_secret=secret%2B%26", request.Body);
        });
        Assert.Collection(handler.Requests,
            request =>
            {
                Assert.Equal("/oauth2/token/", request.Uri.AbsolutePath);
                Assert.Contains("grant_type=authorization_code", request.Body);
                Assert.Contains("code=code%2Ba", request.Body);
            },
            request =>
            {
                Assert.Equal("/oauth2/token/", request.Uri.AbsolutePath);
                Assert.Contains("grant_type=refresh_token", request.Body);
                Assert.Contains("refresh_token=refresh_1", request.Body);
            },
            request =>
            {
                Assert.Equal("/oauth2/revoke/", request.Uri.AbsolutePath);
                Assert.Contains("token=refresh_2", request.Body);
            });
    }

    [Fact]
    public async Task OAuthDoesNotReplayRotatingRefreshTokenOnFailure()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Throttled("0"));
        using var http = new HttpClient(handler);
        var oauth = new CloseOAuthClient("client", "secret", new("https://example.org/callback"), http);
        var error = await Assert.ThrowsAsync<CloseApiException>(() => oauth.RefreshAsync("refresh_example", token));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public void WebhookVerifiesTheOriginalBytesAndReadsEvent()
    {
        var key = "01020304ab";
        var payload = """{ "event": { "id": "ev_demo", "data": { "name": "André" } } }"""u8.ToArray();
        var timestamp = "1795284000";
        var signature = Sign(key, timestamp, payload);
        Assert.True(CloseWebhook.VerifySignature(payload, timestamp, signature, key));

        var changed = """{"event":{"id":"ev_demo","data":{"name":"André"}}}"""u8.ToArray();
        Assert.False(CloseWebhook.VerifySignature(changed, timestamp, signature, key));
        Assert.False(CloseWebhook.VerifySignature(payload, "1795284001", signature, key));
        Assert.False(CloseWebhook.VerifySignature(payload, timestamp, new string('z', 64), key));
        var delivery = CloseWebhook.ReadDelivery(payload);
        Assert.Equal("ev_demo", delivery.Event?.Id);
        Assert.Equal("André", delivery.Event?.Data?.GetProperty("name").GetString());
    }

    [Theory]
    [InlineData(-61, false)]
    [InlineData(-60, true)]
    [InlineData(0, true)]
    [InlineData(60, true)]
    [InlineData(61, false)]
    public void WebhookCanRejectOldAndFutureSignatures(int offsetSeconds, bool expected)
    {
        const string key = "ab12";
        var now = new DateTimeOffset(2026, 11, 21, 18, 0, 0, TimeSpan.Zero);
        var timestamp = now.AddSeconds(offsetSeconds).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        var payload = "{}"u8.ToArray();
        Assert.Equal(expected, CloseWebhook.VerifySignature(payload, timestamp, Sign(key, timestamp, payload),
            key, TimeSpan.FromMinutes(1), new FixedClock(now)));
    }

    private static string Sign(string key, string timestamp, byte[] payload) =>
        Convert.ToHexString(HMACSHA256.HashData(Convert.FromHexString(key).AsSpan(), [.. Encoding.ASCII.GetBytes(timestamp), .. payload]));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
