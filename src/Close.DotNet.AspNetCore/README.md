# Close.DotNet.AspNetCore

Add a signed Close webhook endpoint to an ASP.NET Core app on .NET 10 or 11.

~~~bash
dotnet add package Close.DotNet.AspNetCore --version 1.1.0
~~~

~~~csharp
using Close.DotNet.AspNetCore;

var signingKey = app.Configuration["Close:WebhookSigningKey"]
    ?? throw new InvalidOperationException("Configure the Close webhook signing key.");

app.MapCloseWebhook("/webhooks/close", signingKey, (request, cancellationToken) =>
    queue.EnqueueAsync(request.Event, cancellationToken))
    .AllowAnonymous();
~~~

The endpoint validates the signature over the original body, accepts up to 1 MiB by default, and replies with HTTP 204 after your handler succeeds. It uses the Close subscription key for authentication. Keep event processing idempotent because Close may redeliver an event.

[Setup and runnable sample](https://petefox.github.io/close-dotnet/guides/sync.html)
