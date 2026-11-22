using System.Net;
using System.Text.Json;

namespace Close.DotNet.Tests;

public sealed class EndpointContractTests
{
    public static TheoryData<string, string, string> CrudRoutes()
    {
        var cases = new TheoryData<string, string, string>();
        foreach (var (resource, route) in new Dictionary<string, string>
        {
            ["Leads"] = "lead",
            ["Contacts"] = "contact",
            ["Opportunities"] = "opportunity",
            ["Tasks"] = "task",
            ["Notes"] = "activity/note",
            ["Webhooks"] = "webhook"
        })
        {
            cases.Add(resource, "List", route + "/");
            cases.Add(resource, "Create", route + "/");
            cases.Add(resource, "Get", route + "/example_id/");
            cases.Add(resource, "Update", route + "/example_id/");
            cases.Add(resource, "Delete", route + "/example_id/");
        }
        return cases;
    }

    [Theory]
    [MemberData(nameof(CrudRoutes))]
    public async Task EachCrudMethodUsesItsDocumentedVerbAndPath(string resource, string operation, string route)
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => operation switch
        {
            "List" => JsonResponse.Ok("""{"data":[{"id":"example_id"}],"has_more":false}"""),
            "Delete" => new HttpResponseMessage(HttpStatusCode.NoContent),
            "Create" when resource == "Webhooks" => JsonResponse.Ok("""{"id":"example_id","signature_key":"abcdef"}""", HttpStatusCode.Created),
            _ => JsonResponse.Ok("""{"id":"example_id"}""")
        });
        using var http = new HttpClient(handler);
        var client = TestClient.Create(http);

        CloseObject? result = resource switch
        {
            "Leads" => await Run(client.Leads, operation, new() { Name = "Example" }, new() { Name = "Revised" }, token),
            "Contacts" => await Run(client.Contacts, operation, new() { LeadId = "lead_x", Name = "Example" }, new() { Title = "Manager" }, token),
            "Opportunities" => await Run(client.Opportunities, operation, new() { LeadId = "lead_x" }, new() { Value = 2000 }, token),
            "Tasks" => await Run(client.Tasks, operation, new() { LeadId = "lead_x", Text = "Follow up" }, new() { IsComplete = true }, token),
            "Notes" => await Run(client.Notes, operation, new() { LeadId = "lead_x", Text = "Example" }, new() { Text = "Revised" }, token),
            "Webhooks" => await Run(client.Webhooks, operation, new() { Url = "https://example.org/webhook", Events = [new("lead", "updated")] },
                new() { VerifySsl = true }, token),
            _ => throw new ArgumentOutOfRangeException(nameof(resource))
        };

        var request = Assert.Single(handler.Requests);
        var method = operation switch
        {
            "Create" => HttpMethod.Post,
            "Update" => HttpMethod.Put,
            "Delete" => HttpMethod.Delete,
            _ => HttpMethod.Get
        };
        Assert.Equal(method, request.Method);
        Assert.Equal("/api/v1/" + route, request.Uri.AbsolutePath);
        if (operation == "Delete")
        {
            Assert.Null(result);
            Assert.Null(request.Body);
        }
        else
        {
            Assert.Equal("example_id", result?.Id);
        }
        if (operation is "Create" or "Update")
        {
            Assert.Equal("application/json", request.MediaType);
            Assert.NotEqual("{}", request.Body);
        }
    }

    [Fact]
    public async Task UpdateDistinguishesOmittedNullFalseAndCustomValues()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"id":"lead_delta","custom.cf_region":"West","future_field":true}"""));
        using var http = new HttpClient(handler);
        var client = TestClient.Create(http);
        var update = new UpdateLeadRequest { Description = CloseField<string?>.Set(null), Name = "Delta" };
        update.SetCustomField("cf_region", JsonSerializer.SerializeToElement("West"));

        var lead = await client.Leads.UpdateAsync("lead_delta", update, cancellationToken: token);

        using var json = JsonDocument.Parse(Assert.Single(handler.Requests).Body!);
        Assert.Equal("Delta", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("description").ValueKind);
        Assert.Equal("West", json.RootElement.GetProperty("custom.cf_region").GetString());
        Assert.False(json.RootElement.TryGetProperty("status_id", out _));
        Assert.True(lead.TryGetCustomField("cf_region", out var region));
        Assert.Equal("West", region.GetString());
        Assert.True(lead.AdditionalFields?["future_field"].GetBoolean());
    }

    [Fact]
    public async Task UpdateSerializesExplicitFalseZeroAndEmptyCollections()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"id":"sample"}"""));
        using var http = new HttpClient(handler);
        var client = TestClient.Create(http);
        await client.Tasks.UpdateAsync("task_sample", new() { IsComplete = false }, cancellationToken: token);
        await client.Opportunities.UpdateAsync("oppo_sample", new() { Confidence = 0, Value = 0L }, cancellationToken: token);
        await client.Contacts.UpdateAsync("cont_sample", new() { Emails = Array.Empty<CloseEmail>() }, cancellationToken: token);
        var requests = handler.Requests.ToArray();
        Assert.Equal("""{"is_complete":false}""", requests[0].Body);
        using var opportunity = JsonDocument.Parse(requests[1].Body!);
        Assert.Equal(0, opportunity.RootElement.GetProperty("confidence").GetInt32());
        Assert.Equal(0L, opportunity.RootElement.GetProperty("value").GetInt64());
        Assert.Equal("""{"emails":[]}""", requests[2].Body);
    }

    [Fact]
    public async Task GetPassesFieldProjectionAndAcceptsMissingModelProperties()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""{"display_name":"Delta"}"""));
        using var http = new HttpClient(handler);

        var lead = await TestClient.Create(http).Leads.GetAsync("lead_Delta", new() { Fields = ["display_name", "custom.cf_region"] }, token);

        Assert.Equal("?_fields=display_name%2Ccustom.cf_region", Assert.Single(handler.Requests).Uri.Query);
        Assert.Equal("Delta", lead.DisplayName);
        Assert.Null(lead.Id);
        Assert.Null(lead.Contacts);
    }

    [Fact]
    public async Task EventsGetAndCurrentUserUseDocumentedPathsAndDeserialize()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(request => request.Uri.AbsolutePath.Contains("/event/", StringComparison.Ordinal)
            ? JsonResponse.Ok("""{"id":"ev_1","action":"updated","date_updated":"2026-11-21T12:12:12.123456","data":{"name":"Delta"}}""")
            : JsonResponse.Ok("""{"id":"user_1","organizations":[{"id":"orga_1","name":"Test"}]}"""));
        using var http = new HttpClient(handler);
        var client = TestClient.Create(http);

        var change = await client.Events.GetAsync("ev_1", token);
        var user = await client.Users.GetMeAsync(token);

        Assert.Collection(handler.Requests,
            request => Assert.Equal((HttpMethod.Get, "/api/v1/event/ev_1/"), (request.Method, request.Uri.AbsolutePath)),
            request => Assert.Equal((HttpMethod.Get, "/api/v1/me/"), (request.Method, request.Uri.AbsolutePath)));
        Assert.Equal(TimeSpan.Zero, change.DateUpdated?.Offset);
        Assert.Equal("Delta", change.Data?.GetProperty("name").GetString());
        Assert.Equal("orga_1", Assert.Single(user.Organizations!).Id);
    }

    private static async Task<T?> Run<T, TCreate, TUpdate>(
        CloseResource<T, TCreate, TUpdate> client, string operation, TCreate create, TUpdate update, CancellationToken token)
        where T : CloseObject where TCreate : CloseWriteRequest where TUpdate : CloseWriteRequest
    {
        switch (operation)
        {
            case "List": return Assert.Single((await client.ListPageAsync(cancellationToken: token)).Data);
            case "Create": return await client.CreateAsync(create, cancellationToken: token);
            case "Get": return await client.GetAsync("example_id", cancellationToken: token);
            case "Update": return await client.UpdateAsync("example_id", update, cancellationToken: token);
            case "Delete":
                await client.DeleteAsync("example_id", token);
                return null;
            default: throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }
}
