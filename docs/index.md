# Get started with Close.DotNet

Close.DotNet is an independent SDK for integrating with Close CRM from .NET 10 and 11.

1. Create a Close API key in your development organization.
2. Add the beta package with `dotnet add package Close.DotNet --version 1.0.0-beta.1`.
3. Set `CLOSE_API_KEY` in your environment and make a read-only call.

~~~csharp
using Close.DotNet;

var key = Environment.GetEnvironmentVariable("CLOSE_API_KEY")
    ?? throw new InvalidOperationException("Set CLOSE_API_KEY.");
var close = new CloseClient(key);
var me = await close.Users.GetMeAsync();
Console.WriteLine(me.Id);
~~~

For an ASP.NET Core service with one API key, register the SDK in the host:

~~~csharp
builder.Services.AddCloseClient(
    builder.Configuration["Close:ApiKey"]
        ?? throw new InvalidOperationException("Configure Close:ApiKey."));
~~~

Inject `CloseClient` into an endpoint or service. For multiple OAuth connections, resolve the appropriate credential from the request scope. The SDK reuses the HTTP connection pool and adds credentials on individual requests. When supplying an `HttpClient`, disable automatic redirects and keep credential and middleware configuration under application control.

## Errors and retries

Catch `CloseApiException` to inspect `StatusCode`, `RequestId`, and `RateLimit`. `Details` may include private API data. The client makes up to two retries on rate limits and selected HTTP gateway failures for GET, HEAD, and named advanced searches. Retry waits are capped by the total configured `RetryDelayBudget`, which defaults to 15 seconds. Writes and OAuth token exchanges are sent once. Configure transport and cancellation through `CloseClientOptions`, `HttpClient`, and a cancellation token.

Continue with [core CRM](guides/crm.md), [search](guides/search.md), [OAuth](guides/oauth.md), or [event sync](guides/sync.md).
