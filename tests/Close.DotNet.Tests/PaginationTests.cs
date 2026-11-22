using System.Text.Json;

namespace Close.DotNet.Tests;

public sealed class PaginationTests
{
    [Fact]
    public async Task OffsetStreamAdvancesByReturnedCountWithSameFilters()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 1
            ? JsonResponse.Ok("""{"data":[{"id":"lead_1"}],"has_more":true}""")
            : JsonResponse.Ok("""{"data":[{"id":"lead_2"},{"id":"lead_3"}],"has_more":false}"""));
        using var http = new HttpClient(handler);
        var client = TestClient.Create(http);
        var options = new CloseListOptions
        {
            Limit = 3, Skip = 8, Fields = ["id"], Query = new Dictionary<string, string?> { ["query"] = "A & B" }
        };
        var ids = new List<string?>();
        await foreach (var lead in client.Leads.ListAsync(options, token)) ids.Add(lead.Id);

        Assert.Equal(["lead_1", "lead_2", "lead_3"], ids);
        Assert.Collection(handler.Requests,
            request => Assert.Contains("_skip=8", request.Uri.Query),
            request => Assert.Contains("_skip=9", request.Uri.Query));
        Assert.All(handler.Requests, request =>
        {
            Assert.Contains("_fields=id", request.Uri.Query);
            Assert.Contains("_limit=3", request.Uri.Query);
            Assert.Contains("query=A%20%26%20B", request.Uri.Query);
        });
    }

    [Fact]
    public async Task OffsetStreamRejectsEmptyPageClaimingMoreItems()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"data":[],"has_more":true}"""));
        using var http = new HttpClient(handler);
        await using var iterator = TestClient.Create(http).Contacts.ListAsync(cancellationToken: token).GetAsyncEnumerator(token);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await iterator.MoveNextAsync());
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task EventStreamFollowsNextCursorAndStopsWhenItIsNull()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 1
            ? JsonResponse.Ok("""{"data":[{"id":"ev_1"}],"cursor_next":"opaque&+","cursor_previous":"newer"}""")
            : JsonResponse.Ok("""{"data":[{"id":"ev_2"}],"cursor_next":null}"""));
        using var http = new HttpClient(handler);
        var ids = new List<string?>();
        await foreach (var item in TestClient.Create(http).Events.ListAsync(new()
        {
            Limit = 10, ObjectType = "lead", Cursor = "start"
        }, token))
            ids.Add(item.Id);

        Assert.Equal(["ev_1", "ev_2"], ids);
        Assert.Collection(handler.Requests,
            request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("/api/v1/event/", request.Uri.AbsolutePath);
                Assert.Contains("_cursor=start", request.Uri.Query);
            },
            request => Assert.Contains("_cursor=opaque%26%2B", request.Uri.Query));
        Assert.All(handler.Requests, request => Assert.Contains("object_type=lead", request.Uri.Query));
    }

    [Fact]
    public async Task SearchUsesPostBodyCursorsAndKeepsTypedFiltersAndProjections()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 1
            ? JsonResponse.Ok("""{"data":[{"id":"cont_1","__object_type":"contact"}],"cursor":"second"}""")
            : JsonResponse.Ok("""{"data":[{"id":"cont_2"}],"cursor":null}"""));
        using var http = new HttpClient(handler);
        var search = new CloseSearchRequest
        {
            Query = CloseSearchFilters.And(CloseSearchFilters.ObjectType("contact"),
                CloseSearchFilters.FieldText("contact", "title", "Chief Engineer")),
            Fields = new Dictionary<string, IReadOnlyList<string>> { ["contact"] = ["title"] },
            Limit = 5, ResultsLimit = 10
        };
        var ids = new List<string?>();
        await foreach (var result in TestClient.Create(http).Search.SearchAsync(search, token))
            ids.Add(result.GetProperty("id").GetString());

        Assert.Equal(["cont_1", "cont_2"], ids);
        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/v1/data/search/", request.Uri.AbsolutePath);
            Assert.Empty(request.Uri.Query);
            using var body = JsonDocument.Parse(request.Body!);
            var root = body.RootElement;
            Assert.Equal("title", root.GetProperty("_fields").GetProperty("contact")[0].GetString());
            Assert.Equal("Chief Engineer", root.GetProperty("query").GetProperty("queries")[1]
                .GetProperty("condition").GetProperty("value").GetString());
            Assert.Equal(5, root.GetProperty("_limit").GetInt32());
            Assert.Equal(10, root.GetProperty("results_limit").GetInt32());
            if (request.Attempt == 1) Assert.False(root.TryGetProperty("cursor", out _));
            else Assert.Equal("second", root.GetProperty("cursor").GetString());
        });
    }

    [Fact]
    public async Task SearchAcceptsCopiedFiltersAndCompleteJsonRequests()
    {
        var token = TestContext.Current.CancellationToken;
        using var adapter = new ScriptedHandler(_ => JsonResponse.Ok("""{"data":[],"cursor":null}"""));
        using var http = new HttpClient(adapter);
        var client = TestClient.Create(http);
        using var tree = JsonDocument.Parse("""{"type":"object_type","object_type":"lead"}""");
        using var full = JsonDocument.Parse("""{"query":{"type":"object_type","object_type":"lead"},"future_option":true}""");
        await client.Search.SearchJsonPageAsync(tree.RootElement, token);
        await client.Search.SearchJsonPageAsync(full.RootElement, token);
        Assert.Collection(adapter.Requests,
            request => Assert.Equal("""{"query":{"type":"object_type","object_type":"lead"}}""", request.Body),
            request => Assert.Equal(full.RootElement.GetRawText(), request.Body));
    }

    [Fact]
    public async Task SearchStopsWhenCloseRepeatsTheCursor()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"data":[],"cursor":"same"}"""));
        using var http = new HttpClient(handler);
        await using var iterator = TestClient.Create(http).Search.SearchAsync(new() { Cursor = "same" }, token).GetAsyncEnumerator(token);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await iterator.MoveNextAsync());
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task EndingEnumerationEarlyNeverFetchesAnotherPage()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"data":[{"id":"lead_1"}],"has_more":true}"""));
        using var http = new HttpClient(handler);
        await using (var iterator = TestClient.Create(http).Leads.ListAsync(cancellationToken: token).GetAsyncEnumerator(token))
        {
            Assert.True(await iterator.MoveNextAsync());
            Assert.Equal("lead_1", iterator.Current.Id);
        }
        Assert.Single(handler.Requests);
    }
}
