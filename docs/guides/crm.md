# Read and update core CRM records

Run the [console sample](https://github.com/petefox/close-dotnet/tree/main/samples/Quickstart) with `CLOSE_API_KEY` set for a development organization:

~~~bash
dotnet run --project samples/Quickstart
~~~

By default it prints the authenticated user ID and up to five leads. Request projections on large lists:

~~~csharp
var firstPage = await close.Leads.ListPageAsync(new()
{
    Limit = 20,
    Fields = ["id", "display_name", "custom.cf_region"]
}, cancellationToken);
~~~

Fields outside the projection can be null. Use `ListAsync` to stream forward automatically or advance a manual offset by `Data.Count` while `HasMore` is true. Use [advanced search](search.md) or the Close Export API for datasets that exceed Close's offset limits.

Update a lead in your development organization:

~~~bash
dotnet run --project samples/Quickstart -- update lead_example "Updated by the SDK tutorial"
~~~

An update includes only specified fields. Set a nullable field to JSON null to clear it when Close permits clearing that field. Send an empty collection to replace a list with an empty list.

~~~csharp
var update = new UpdateLeadRequest
{
    Name = "Example Company",
    Url = CloseField<string?>.Set(null)
};
update.SetCustomField("cf_region", System.Text.Json.JsonSerializer.SerializeToElement("West"));
var updated = await close.Leads.UpdateAsync(leadId, update, cancellationToken: cancellationToken);
~~~

Read a custom field with `updated.TryGetCustomField("cf_region", out var region)`. The value is a `JsonElement`, so it retains the type configured in Close.

For endpoints awaiting named methods, pass a relative API route and separate unescaped query values:

~~~csharp
var response = await close.SendAsync(HttpMethod.Get, "pipeline/",
    cancellationToken: cancellationToken);
Console.WriteLine(response.Data);
~~~
