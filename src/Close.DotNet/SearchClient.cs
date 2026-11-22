using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Close.DotNet;

/// <summary>Request for the Close Advanced Filtering API.</summary>
public sealed record CloseSearchRequest
{
    /// <summary>Close filter tree. Use <see cref="CloseSearchFilters"/> or paste a query from Close.</summary>
    public JsonElement? Query { get; init; }

    /// <summary>Field projections keyed by Close object type.</summary>
    [JsonPropertyName("_fields")] public IReadOnlyDictionary<string, IReadOnlyList<string>>? Fields { get; init; }

    /// <summary>Requested page size, up to 200.</summary>
    [JsonPropertyName("_limit")] public int? Limit { get; init; }

    /// <summary>Opaque cursor reported by the previous page.</summary>
    public string? Cursor { get; init; }

    /// <summary>Total result limit across pages. Use zero with IncludeCounts for count-only requests.</summary>
    public int? ResultsLimit { get; init; }

    /// <summary>Requests match counts.</summary>
    public bool? IncludeCounts { get; init; }

    /// <summary>Sort specifications in Close query format.</summary>
    public IReadOnlyList<JsonElement>? Sort { get; init; }
}

/// <summary>A page of heterogeneous projection results.</summary>
public sealed class CloseSearchPage
{
    /// <summary>Individual results, including the __object_type discriminator.</summary>
    public IReadOnlyList<JsonElement> Data { get; init; } = [];

    /// <summary>Next-page cursor, which is short lived.</summary>
    public string? Cursor { get; init; }

    /// <summary>Match counts if requested.</summary>
    public CloseSearchCount? Count { get; init; }
}

/// <summary>Match counts reported by advanced search.</summary>
public sealed class CloseSearchCount
{
    /// <summary>Number of matches reported by Close.</summary>
    public int? Total { get; init; }

    /// <summary>Number of matches within the search result limit.</summary>
    public int? Limited { get; init; }

    /// <summary>Other count fields returned by Close.</summary>
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}

/// <summary>Constructs commonly used Close advanced-search filter nodes.</summary>
public static class CloseSearchFilters
{
    /// <summary>Matches objects of the specified Close type, such as lead or contact.</summary>
    public static JsonElement ObjectType(string objectType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        return Element(new JsonObject { ["type"] = "object_type", ["object_type"] = objectType });
    }

    /// <summary>Matches text in a field on the specified object type.</summary>
    public static JsonElement FieldText(string objectType, string fieldName, string text, string mode = "full_words")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(mode);
        return Element(new JsonObject
        {
            ["type"] = "field_condition",
            ["field"] = new JsonObject { ["type"] = "regular_field", ["object_type"] = objectType, ["field_name"] = fieldName },
            ["condition"] = new JsonObject { ["type"] = "text", ["mode"] = mode, ["value"] = text }
        });
    }

    /// <summary>Matches every child filter.</summary>
    public static JsonElement And(params JsonElement[] filters) => Join("and", filters);

    /// <summary>Matches any child filter.</summary>
    public static JsonElement Or(params JsonElement[] filters) => Join("or", filters);

    private static JsonElement Join(string type, JsonElement[] filters)
    {
        ArgumentNullException.ThrowIfNull(filters);
        if (filters.Length == 0 || filters.Any(filter => filter.ValueKind != JsonValueKind.Object))
            throw new ArgumentException("Provide one or more Close filter objects.", nameof(filters));
        return Element(new JsonObject
        {
            ["type"] = type,
            ["queries"] = new JsonArray(filters.Select(filter => JsonNode.Parse(filter.GetRawText())).ToArray())
        });
    }

    internal static JsonElement Element(JsonObject value)
    {
        using var document = JsonDocument.Parse(value.ToJsonString());
        return document.RootElement.Clone();
    }
}

/// <summary>Queries the Close Advanced Filtering API.</summary>
public sealed class SearchClient
{
    private readonly CloseTransport transport;

    internal SearchClient(CloseTransport transport) => this.transport = transport;

    /// <summary>Retrieves one page of a typed search request.</summary>
    public Task<CloseSearchPage> SearchPageAsync(CloseSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Limit is { } limit)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, 200);
        }
        if (request.ResultsLimit is { } total) ArgumentOutOfRangeException.ThrowIfNegative(total);
        return SendAsync(JsonSerializer.SerializeToUtf8Bytes(request, CloseJson.Type<CloseSearchRequest>()), cancellationToken);
    }

    /// <summary>Retrieves a page from a copied Close filter tree or complete JSON request.</summary>
    public Task<CloseSearchPage> SearchJsonPageAsync(JsonElement request, CancellationToken cancellationToken = default)
    {
        if (request.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Supply a JSON object containing a Close filter or request.", nameof(request));
        if (request.TryGetProperty("type", out _) && !request.TryGetProperty("query", out _))
            return SearchPageAsync(new() { Query = request }, cancellationToken);
        return SendAsync(JsonSerializer.SerializeToUtf8Bytes(request, CloseJson.Default.JsonElement), cancellationToken);
    }

    /// <summary>Streams search results using Close's short-lived cursors.</summary>
    public IAsyncEnumerable<JsonElement> SearchAsync(CloseSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CloseRoutes.StreamCursorAsync(request.Cursor, async (cursor, token) =>
        {
            var page = await SearchPageAsync(request with { Cursor = cursor }, token).ConfigureAwait(false);
            return (page.Data, page.Cursor);
        }, cancellationToken);
    }

    private async Task<CloseSearchPage> SendAsync(byte[] body, CancellationToken token) =>
        (await transport.SendAsync(HttpMethod.Post, "data/search/", null, body, CloseJson.Type<CloseSearchPage>(), true, token)
            .ConfigureAwait(false)).RequireData();
}
