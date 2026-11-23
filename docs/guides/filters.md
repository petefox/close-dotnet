# Use typed filters and custom fields

Pass an endpoint's options type to an existing SDK method. Use `Fields` to select the returned properties and `Limit` to control the page size.

~~~csharp
await foreach (var contact in close.Contacts.ListAsync(new ContactListOptions
{
    LeadId = leadId,
    Fields = ["id", "name"],
    Limit = 25
}, cancellationToken))
{
    Console.WriteLine(contact.Id);
}
~~~

Dates, booleans, and documented enum values use .NET types. For example, filter opportunities by multiple status types and a date range.

~~~csharp
var page = await close.Opportunities.ListPageAsync(new OpportunityListOptions
{
    StatusTypes = [OpportunityStatusType.Active, OpportunityStatusType.Won],
    DateWonOnOrAfter = new DateOnly(2026, 1, 1),
    IsStalled = false
}, cancellationToken);
~~~

Use the inherited `Query` dictionary for advanced or newly introduced parameters. Its values remain unescaped strings. A specified typed filter takes precedence when both options set the same wire key. An unset typed filter preserves the dictionary value. Pagination and field projections follow their existing rules.

~~~csharp
var options = new OpportunityListOptions
{
    LeadId = leadId,
    Query = new Dictionary<string, string?>
    {
        ["_order_by"] = "-date_updated"
    }
};
~~~

## Set and read organization-defined fields

Retrieve a field ID from the applicable custom-field definition client, such as `LeadCustomFields`, `CustomObjectFields`, or `ActivityCustomFields`. Use the ID with `SetCustomField` and `TryGetCustomField`. JSON values preserve arrays, numbers, strings, booleans, and null. The same helpers work for core CRM records, custom objects, and custom activities.

~~~csharp
using System.Text.Json;
using Close.DotNet.Models;

var update = new CustomObjectsUpdateRequest();
update.SetCustomField(fieldId, JsonSerializer.SerializeToElement("West"));

var record = await close.CustomObjects.UpdateAsync(customObjectId, update,
    cancellationToken: cancellationToken);
if (record.TryGetCustomField(fieldId, out var region))
    Console.WriteLine(region.GetString());
~~~

To clear a field in an update, pass a JSON null. The 1.0 properties named after custom fields in Close's example payloads still work for compatibility. The helpers take the field IDs from your organization.

[Close filter parameters](https://developer.close.com/api/overview/filter-parameters)
