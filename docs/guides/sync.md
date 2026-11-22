# Consume events and webhooks

Use `ListPageAsync` for event replay checkpoints, or stream toward older events:

~~~csharp
await foreach (var change in close.Events.ListAsync(new()
{
    ObjectType = "lead",
    Action = "updated",
    Limit = 20
}, cancellationToken))
{
    Console.WriteLine(change.Id);
}
~~~

For live deliveries, run the [ASP.NET Core sample](https://github.com/petefox/close-dotnet/tree/main/samples/OAuthAndWebhooks) on an HTTPS URL reachable by Close. Register a subscription from an authorized connection to your development organization:

~~~csharp
var subscription = await close.Webhooks.CreateAsync(new()
{
    Url = "https://your-development-host.example/webhook",
    Events = [new("lead", "updated")]
}, cancellationToken: cancellationToken);
~~~

Install the optional ASP.NET Core integration and set `Close:WebhookSigningKey` to the subscription's `SignatureKey` in user secrets or the production secret store.

~~~bash
dotnet add package Close.DotNet.AspNetCore --version 1.0.0
~~~

~~~csharp
using Close.DotNet.AspNetCore;

var key = app.Configuration["Close:WebhookSigningKey"]
    ?? throw new InvalidOperationException("Configure the Close signing key.");

app.MapCloseWebhook("/webhook", key, async (request, token) =>
{
    await queue.EnqueueAsync(request.Event, token);
}).AllowAnonymous();
~~~

Set the subscription URL to the mapped route. The helper validates the signature against the original bytes before invoking your handler. It limits incoming bodies to 1 MiB by default and returns HTTP 204 after your callback completes. A handler exception returns an error so Close can retry. Use `CloseWebhookEndpointOptions` to change the body limit or set an optional signature freshness window.

The sample uses the helper to write each event once to an encrypted inbox in `.local/inbox/`. Replace that inbox with your application's durable queue in a deployed receiver. The key resolver overload supports separate subscriptions with keys from trusted route configuration.

Close may deliver an event more than once. Keep processing idempotent and use the event log for missed deliveries within its documented retention window. Remove the test subscription with `close.Webhooks.DeleteAsync(subscription.Id!)` after the tutorial.

[Close webhook delivery and signatures](https://developer.close.com/api/resources/webhooks/)
