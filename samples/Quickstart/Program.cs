using Close.DotNet;

var key = Environment.GetEnvironmentVariable("CLOSE_API_KEY")
    ?? throw new InvalidOperationException("Set CLOSE_API_KEY for a Close development organization.");
var close = new CloseClient(key);

var me = await close.Users.GetMeAsync();
Console.WriteLine($"Authenticated as {me.Id}.");

if (args is ["update", var leadId, var description])
{
    var lead = await close.Leads.UpdateAsync(leadId, new() { Description = description });
    Console.WriteLine($"Updated {lead.Id}.");
    return;
}

if (args is ["search", var title])
{
    var search = new CloseSearchRequest
    {
        Query = CloseSearchFilters.And(
            CloseSearchFilters.ObjectType("contact"),
            CloseSearchFilters.FieldText("contact", "title", title)),
        Fields = new Dictionary<string, IReadOnlyList<string>> { ["contact"] = ["id", "name"] },
        ResultsLimit = 10
    };
    await foreach (var result in close.Search.SearchAsync(search))
        Console.WriteLine(result.GetProperty("id").GetString());
    return;
}

var leads = await close.Leads.ListPageAsync(new() { Limit = 5, Fields = ["id", "display_name"] });
foreach (var lead in leads.Data)
    Console.WriteLine($"{lead.Id} {lead.DisplayName}");
