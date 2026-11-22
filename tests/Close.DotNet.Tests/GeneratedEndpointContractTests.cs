using System.Net;

namespace Close.DotNet.Tests;

public sealed partial class GeneratedEndpointContractTests
{
    public static TheoryData<string> Operations => new(Contracts.Select(contract => contract.Operation));

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task NamedEndpointUsesDocumentedMethodRouteAndBody(string operation)
    {
        var contract = Assert.Single(Contracts, item => item.Operation == operation);
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => contract.Fixture is { } fixture
            ? JsonResponse.Ok(fixture) : new HttpResponseMessage(HttpStatusCode.NoContent));
        using var http = new HttpClient(handler);

        await contract.Invoke(TestClient.Create(http), token);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(contract.Method, request.Method);
        Assert.Equal("/api/v1" + contract.Route, request.Uri.AbsolutePath);
        Assert.Equal(contract.HasBody, request.Body is not null);
        if (contract.HasBody) Assert.Equal("application/json", request.MediaType);
        if (contract.Query is { } query) Assert.Equal("?" + query, request.Uri.Query);
    }

    private sealed record Contract(string Operation, HttpMethod Method, string Route, bool HasBody,
        string? Fixture, string? Query, Func<CloseClient, CancellationToken, Task> Invoke);
}
