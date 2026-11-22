# Find contacts with advanced search

This query finds contacts whose title contains a word or phrase:

~~~csharp
var search = new CloseSearchRequest
{
    Query = CloseSearchFilters.And(
        CloseSearchFilters.ObjectType("contact"),
        CloseSearchFilters.FieldText("contact", "title", "Engineer")),
    Fields = new Dictionary<string, IReadOnlyList<string>>
    {
        ["contact"] = ["id", "name", "title"]
    },
    ResultsLimit = 10
};
await foreach (var item in close.Search.SearchAsync(search, cancellationToken))
    Console.WriteLine(item.GetProperty("id").GetString());
~~~

Because one search can match different object types, each result retains its JSON shape and `__object_type` discriminator. Use `TryGetProperty` for optional and projected fields. For long-running processing, fetch and buffer a page before doing work that could exceed Close's 30-second cursor lifetime.

To reuse a filter from the Close UI, choose **Copy Filters** on a CRM results page and save the JSON as `filters.json`.

~~~csharp
using var filters = System.Text.Json.JsonDocument.Parse(
    await File.ReadAllTextAsync("filters.json", cancellationToken));
var page = await close.Search.SearchJsonPageAsync(filters.RootElement, cancellationToken);
~~~

The [console sample](https://github.com/petefox/close-dotnet/tree/main/samples/Quickstart) runs a typed query with `dotnet run --project samples/Quickstart -- search Engineer`.

[Close filters and cursor limits](https://developer.close.com/api/resources/advanced-filtering/search)
