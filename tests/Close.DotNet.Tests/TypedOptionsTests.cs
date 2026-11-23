using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Close.DotNet.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace Close.DotNet.Tests;

public sealed class TypedOptionsTests
{
    [Fact]
    public async Task TypedContactFiltersMergeWithTheExistingDictionaryAndPagination()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 1
            ? JsonResponse.Ok("""{"data":[{"id":"cont_1"}],"has_more":true}""")
            : JsonResponse.Ok("""{"data":[{"id":"cont_2"}],"has_more":false}"""));
        using var http = new HttpClient(handler);
        var fallback = new Dictionary<string, string?>
        {
            ["lead_id"] = "lead_raw", ["advanced"] = "a+b & c", ["omit"] = null
        };
        CloseListOptions options = new ContactListOptions
        {
            LeadId = "lead_typed", Skip = 3, Limit = 2, Fields = ["id"], Query = fallback
        };
        var contacts = new List<string?>();
        await foreach (var contact in TestClient.Create(http).Contacts.ListAsync(options, token))
            contacts.Add(contact.Id);

        Assert.Equal(["cont_1", "cont_2"], contacts);
        Assert.Collection(handler.Requests,
            request => Assert.Equal("3", QueryHelpers.ParseQuery(request.Uri.Query)["_skip"].ToString()),
            request => Assert.Equal("4", QueryHelpers.ParseQuery(request.Uri.Query)["_skip"].ToString()));
        Assert.All(handler.Requests, request =>
        {
            var query = QueryHelpers.ParseQuery(request.Uri.Query);
            Assert.Equal("lead_typed", query["lead_id"].ToString());
            Assert.Equal("a+b & c", query["advanced"].ToString());
            Assert.Equal("2", query["_limit"].ToString());
            Assert.Equal("id", query["_fields"].ToString());
            Assert.False(query.ContainsKey("omit"));
        });
        Assert.Equal("lead_raw", fallback["lead_id"]);
    }

    [Fact]
    public async Task OpportunityFiltersUseCloseDateBooleanEnumAndCsvFormats()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"data":[],"has_more":false}"""));
        using var http = new HttpClient(handler);
        await TestClient.Create(http).Opportunities.ListPageAsync(new OpportunityListOptions
        {
            DateWonOnOrAfter = new DateOnly(2026, 11, 22),
            DateCreatedOnOrAfter = new DateOnly(2026, 11, 20),
            DateUpdatedBefore = new DateTimeOffset(2026, 11, 22, 9, 30, 0, TimeSpan.FromHours(-8)),
            StatusTypes = [OpportunityStatusType.Active, OpportunityStatusType.Won],
            ValuePeriod = OpportunityValuePeriod.OneTime,
            UserIds = [],
            IsStalled = false,
            Query = new Dictionary<string, string?> { ["user_id__in"] = "user_fallback" }
        }, token);

        var query = QueryHelpers.ParseQuery(Assert.Single(handler.Requests).Uri.Query);
        Assert.Equal("2026-11-22", query["date_won__gte"].ToString());
        Assert.Equal("2026-11-20", query["date_created__gte"].ToString());
        Assert.Equal("2026-11-22T09:30:00.0000000-08:00", query["date_updated__lt"].ToString());
        Assert.Equal("active,won", query["status_type__in"].ToString());
        Assert.Equal("one_time", query["value_period"].ToString());
        Assert.Equal("false", query["is_stalled"].ToString());
        Assert.Equal("user_fallback", query["user_id__in"].ToString());
    }

    [Fact]
    public async Task CommentThreadListSendsRepeatedQueryKeysWithoutLosingValues()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"data":[],"has_more":false}"""));
        using var http = new HttpClient(handler);
        await TestClient.Create(http).Comments.ListThreadsPageAsync(new CommentsListThreadsOptions
        {
            Ids = ["comthr_1", "comthr_a+b"],
            Query = new Dictionary<string, string?> { ["ids"] = "legacy", ["object_ids"] = "unmodeled" }
        }, token);

        var query = QueryHelpers.ParseQuery(Assert.Single(handler.Requests).Uri.Query);
        Assert.Equal(new[] { "comthr_1", "comthr_a+b" }, query["ids"].ToArray());
        Assert.Equal("unmodeled", query["object_ids"].ToString());
    }

    [Fact]
    public async Task ExistingEventOptionsUseTheirTypedFiltersAndKeepCursorSemantics()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 1
            ? JsonResponse.Ok("""{"data":[],"cursor_next":"evt_cursor"}""")
            : JsonResponse.Ok("""{"data":[]}"""));
        using var http = new HttpClient(handler);
        await foreach (var _ in TestClient.Create(http).Events.ListAsync(new()
        {
            RequestId = "request_example",
            UpdatedSince = new DateTimeOffset(2026, 11, 22, 0, 0, 0, TimeSpan.Zero),
            Query = new Dictionary<string, string?> { ["_cursor"] = "discarded" }
        }, token)) { }

        Assert.Collection(handler.Requests,
            request => Assert.False(QueryHelpers.ParseQuery(request.Uri.Query).ContainsKey("_cursor")),
            request => Assert.Equal("evt_cursor", QueryHelpers.ParseQuery(request.Uri.Query)["_cursor"].ToString()));
        Assert.All(handler.Requests, request =>
        {
            var query = QueryHelpers.ParseQuery(request.Uri.Query);
            Assert.Equal("request_example", query["request_id"].ToString());
            Assert.Equal("2026-11-22T00:00:00.0000000+00:00", query["date_updated__gte"].ToString());
        });
    }

    [Fact]
    public async Task CustomObjectWriteAndActivityReadUseOrganizationFieldIds()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""
            {"id":"example_id","custom.cf_team":["billing","support"],"custom.cf_notes":null}
            """));
        using var http = new HttpClient(handler);
        var close = TestClient.Create(http);
        var request = new CustomObjectsUpdateRequest();
        using (var data = JsonDocument.Parse("""["billing","support"]"""))
            request.SetCustomField("cf_team", data.RootElement);
        request.SetCustomField("cf_notes", JsonSerializer.SerializeToElement<string?>(null));

        var record = await close.CustomObjects.UpdateAsync("cobj_example", request, cancellationToken: token);
        var activity = await close.CustomActivities.GetAsync("acti_example", cancellationToken: token);

        using var body = JsonDocument.Parse(handler.Requests.First().Body!);
        Assert.Equal("billing", body.RootElement.GetProperty("custom.cf_team")[0].GetString());
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("custom.cf_notes").ValueKind);
        Assert.True(record.TryGetCustomField("cf_team", out var team));
        Assert.Equal("support", team[1].GetString());
        Assert.True(activity.TryGetCustomField("cf_notes", out var notes));
        Assert.Equal(JsonValueKind.Null, notes.ValueKind);
        Assert.False(activity.TryGetCustomField("cf_absent", out _));
    }

    [Fact]
    public async Task LegacyExamplePropertyRemainsCallableAndKeepsItsJsonName()
    {
        var token = TestContext.Current.CancellationToken;
        const string field = "cf_jnxTQDKnSxCEBtD5dvZdNgTzC3CqeyWUyU349PBA9Wf";
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""
            {"id":"cobj_example","custom.cf_jnxTQDKnSxCEBtD5dvZdNgTzC3CqeyWUyU349PBA9Wf":"example"}
            """));
        using var http = new HttpClient(handler);
        var record = await TestClient.Create(http).CustomObjects.CreateAsync(new()
        {
            CustomCfJnxTQDKnSxCEBtD5dvZdNgTzC3CqeyWUyU349PBA9Wf = "example"
        }, cancellationToken: token);

        using var body = JsonDocument.Parse(Assert.Single(handler.Requests).Body!);
        Assert.Equal("example", body.RootElement.GetProperty("custom." + field).GetString());
        Assert.Equal("example", record.CustomCfJnxTQDKnSxCEBtD5dvZdNgTzC3CqeyWUyU349PBA9Wf);
        Assert.True(record.TryGetCustomField(field, out var value));
        Assert.Equal("example", value.GetString());
        Assert.Equal(EditorBrowsableState.Never, typeof(CustomObjectsCreateResponse)
            .GetProperty(nameof(CustomObjectsCreateResponse.CustomCfJnxTQDKnSxCEBtD5dvZdNgTzC3CqeyWUyU349PBA9Wf))?
            .GetCustomAttribute<EditorBrowsableAttribute>()?.State);
    }
}
