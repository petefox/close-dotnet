using System.Text.Json;

namespace Close.DotNet.Tests;

public sealed class ExtendedResourceTests
{
    [Fact]
    public async Task GeneratedUpdatePreservesExplicitNullAndOmitsUntouchedFields()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"id":"play_test","description":null}"""));
        using var http = new HttpClient(handler);

        var playbook = await TestClient.Create(http).Playbooks.UpdateAsync("play_test", new()
        {
            Description = CloseField<string?>.Set(null)
        }, cancellationToken: token);

        using var body = JsonDocument.Parse(Assert.Single(handler.Requests).Body!);
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("description").ValueKind);
        Assert.False(body.RootElement.TryGetProperty("name", out _));
        Assert.Equal("play_test", playbook.Id);
    }

    [Fact]
    public async Task GeneratedListStreamsMultipleTypedPages()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Attempt == 1
            ? JsonResponse.Ok("""{"data":[{"id":"expo_1","status":"done"}],"has_more":true}""")
            : JsonResponse.Ok("""{"data":[{"id":"expo_2","status":"running"}],"has_more":false}"""));
        using var http = new HttpClient(handler);
        var ids = new List<string?>();
        await foreach (var export in TestClient.Create(http).Exports.ListAsync(new() { Limit = 2, Skip = 7 }, token))
            ids.Add(export.Id);

        Assert.Equal(["expo_1", "expo_2"], ids);
        Assert.Collection(handler.Requests,
            request => Assert.Equal("?_skip=7&_limit=2", request.Uri.Query),
            request => Assert.Equal("?_skip=8&_limit=2", request.Uri.Query));
    }
}
