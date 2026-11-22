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

Store the returned `SignatureKey` as `Close:WebhookSigningKey` in the receiver's secret configuration. The sample verifies the original bytes against the signature headers, stores the event once per ID, and replies with HTTP 202. Incoming bodies are limited to 1 MiB. The sample's encrypted inbox is in `.local/inbox/`. A deployed receiver can replace that with the application's durable queue.

Close may deliver an event more than once. Keep processing idempotent and use the event log for missed deliveries within its documented retention window. Remove the test subscription with `close.Webhooks.DeleteAsync(subscription.Id!)` after the tutorial.

[Close webhook delivery and signatures](https://developer.close.com/api/resources/webhooks/)
