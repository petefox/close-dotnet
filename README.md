# Close.DotNet

An independent C# SDK for the [Close CRM REST API](https://developer.close.com/). It targets .NET 10 and works on .NET 11.

## Install

~~~bash
dotnet add package Close.DotNet --version 1.0.0
~~~

## Make your first request

Create an API key in your Close development organization under **Settings → Developer → API Keys**. Set it as the `CLOSE_API_KEY` environment variable, then run:

~~~csharp
using Close.DotNet;

var apiKey = Environment.GetEnvironmentVariable("CLOSE_API_KEY")
    ?? throw new InvalidOperationException("Set CLOSE_API_KEY.");

var close = new CloseClient(apiKey);
var me = await close.Users.GetMeAsync();
Console.WriteLine(me.Id);
~~~

List leads with an async stream:

~~~csharp
await foreach (var lead in close.Leads.ListAsync(new()
{
    Limit = 25,
    Fields = ["id", "display_name"]
}))
{
    Console.WriteLine($"{lead.Id} {lead.DisplayName}");
}
~~~

The SDK has named methods for all 308 documented REST operations, plus OAuth, webhook verification, and streamed file transfers. Use `CloseClient.SendAsync` when you need to call a newly documented route ahead of the next SDK release.

- [Tutorials and C# API reference](https://petefox.github.io/close-dotnet/)
- [Endpoint coverage](https://github.com/petefox/close-dotnet/blob/main/docs/coverage.md)
- [Runnable samples](https://github.com/petefox/close-dotnet/tree/main/samples)
- [Contributing](https://github.com/petefox/close-dotnet/blob/main/CONTRIBUTING.md)

MIT licensed. This community SDK is independent of Close.
