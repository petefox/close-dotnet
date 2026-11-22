using System.Security.Cryptography;
using System.Text;
using Close.DotNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var localData = Path.Combine(builder.Environment.ContentRootPath, ".local");
var keyDirectory = Directory.CreateDirectory(Path.Combine(localData, "keys"));
builder.Services.AddDataProtection().PersistKeysToFileSystem(keyDirectory);
builder.Services.AddSingleton(provider => new SampleStore(localData, provider.GetRequiredService<IDataProtectionProvider>()));

var app = builder.Build();
var redirect = app.Configuration["Close:RedirectUri"];
var oauth = app.Configuration["Close:ClientId"] is { } clientId &&
            app.Configuration["Close:ClientSecret"] is { } secret && redirect is { }
    ? new CloseOAuthClient(clientId, secret, new Uri(redirect)) : null;
var webhookKey = app.Configuration["Close:WebhookSigningKey"];
var browserCookie = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/" };

app.MapGet("/", () => Results.Text("Open /connect to authorize the Close test organization, then /me to make an API call."));
app.MapGet("/connect", (HttpContext context, SampleStore store) =>
{
    if (oauth is null) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

    var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    store.StartAuthorization(state);
    context.Response.Cookies.Append("close_oauth_state", state, new()
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/callback/close",
        MaxAge = TimeSpan.FromMinutes(10)
    });
    return Results.Redirect(oauth.CreateAuthorizationUri(state).AbsoluteUri);
});

app.MapGet("/callback/close", async (HttpContext context, SampleStore store, string? code, string? state, string? error) =>
{
    if (oauth is null) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    context.Response.Cookies.Delete("close_oauth_state", new() { Secure = true, Path = "/callback/close" });
    var expected = context.Request.Cookies["close_oauth_state"];
    if (state is null || expected is null || state.Length != expected.Length ||
        !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(state), Encoding.UTF8.GetBytes(expected)) ||
        !store.ConsumeAuthorization(state))
        return Results.BadRequest("Invalid or expired OAuth state.");
    if (error is not null || string.IsNullOrWhiteSpace(code)) return Results.BadRequest("Close authorization was declined.");

    using var exchange = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var tokens = await oauth.ExchangeCodeAsync(code, exchange.Token);
    var connectionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    await store.SaveConnectionAsync(connectionId, tokens);
    context.Response.Cookies.Append("close_connection", connectionId, browserCookie);
    return Results.Redirect("/me");
});

app.MapGet("/me", async (HttpContext context, SampleStore store) =>
{
    if (oauth is null || context.Request.Cookies["close_connection"] is not { } connectionId)
        return Results.Unauthorized();

    var credential = CloseCredential.AccessTokenProvider(
        cancellationToken => store.GetAccessTokenAsync(connectionId, oauth, cancellationToken));
    try
    {
        var user = await new CloseClient(credential).Users.GetMeAsync(context.RequestAborted);
        return Results.Ok(new { user.Id });
    }
    catch (ConnectionUnavailableException)
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/webhook", async (HttpRequest request, SampleStore store, CancellationToken token) =>
{
    if (webhookKey is null) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

    using var buffer = new MemoryStream();
    await request.Body.CopyToAsync(buffer, token);
    var payload = buffer.ToArray();
    if (!CloseWebhook.VerifySignature(payload, request.Headers[CloseWebhook.TimestampHeader].ToString(),
        request.Headers[CloseWebhook.SignatureHeader].ToString(), webhookKey))
        return Results.Unauthorized();

    CloseWebhookDelivery delivery;
    try
    {
        delivery = CloseWebhook.ReadDelivery(payload);
    }
    catch (System.Text.Json.JsonException)
    {
        return Results.BadRequest("Invalid Close delivery.");
    }
    if (delivery.Event?.Id is not { } eventId) return Results.BadRequest("Missing Close event ID.");

    await store.EnqueueEventAsync(eventId, payload, token);
    return Results.Accepted();
}).WithMetadata(new RequestSizeLimitAttribute(1024 * 1024));

app.Run();
