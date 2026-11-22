using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Close.DotNet;

/// <summary>Options for one page of the Close event log.</summary>
public sealed class CloseEventListOptions : CloseRequestOptions
{
    /// <summary>Opaque pagination cursor returned by Close.</summary>
    public string? Cursor { get; init; }

    /// <summary>Requested page size, up to 50.</summary>
    public int? Limit { get; init; }

    /// <summary>Filter by Close object type.</summary>
    public string? ObjectType { get; init; }

    /// <summary>Filter by event action.</summary>
    public string? Action { get; init; }

    /// <summary>Filter for updates at or after this timestamp.</summary>
    public DateTimeOffset? UpdatedSince { get; init; }
}

/// <summary>A page of the Close event log.</summary>
public sealed class CloseEventPage
{
    /// <summary>Events in this page.</summary>
    public IReadOnlyList<CloseEvent> Data { get; init; } = [];

    /// <summary>Cursor for earlier events.</summary>
    public string? CursorNext { get; init; }

    /// <summary>Cursor for later events.</summary>
    public string? CursorPrevious { get; init; }

    /// <summary>Additional response fields.</summary>
    [JsonExtensionData] public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}

/// <summary>Reads Close's event log.</summary>
public sealed class EventsClient
{
    private readonly CloseTransport transport;

    internal EventsClient(CloseTransport transport) => this.transport = transport;

    /// <summary>Retrieves a single event by ID.</summary>
    public async Task<CloseEvent> GetAsync(string id, CancellationToken cancellationToken = default) =>
        (await transport.SendAsync(HttpMethod.Get, CloseRoutes.Item("event/", id), null, null,
            CloseJson.Type<CloseEvent>(), true, cancellationToken).ConfigureAwait(false)).RequireData();

    /// <summary>Retrieves one page of the event log.</summary>
    public Task<CloseEventPage> ListPageAsync(CloseEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new();
        return ListPageCoreAsync(Query(options), options.Cursor, cancellationToken);
    }

    /// <summary>Streams from the starting cursor towards older events.</summary>
    public IAsyncEnumerable<CloseEvent> ListAsync(CloseEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new();
        var query = Query(options);
        return CloseRoutes.StreamCursorAsync(options.Cursor, async (cursor, token) =>
        {
            var page = await ListPageCoreAsync(new(query, StringComparer.Ordinal), cursor, token).ConfigureAwait(false);
            return (page.Data, page.CursorNext);
        }, cancellationToken);
    }

    private async Task<CloseEventPage> ListPageCoreAsync(Dictionary<string, string?> query, string? cursor, CancellationToken token)
    {
        if (cursor is not null) query["_cursor"] = cursor;
        return (await transport.SendAsync(HttpMethod.Get, "event/", query, null,
            CloseJson.Type<CloseEventPage>(), true, token).ConfigureAwait(false)).RequireData();
    }

    private static Dictionary<string, string?> Query(CloseEventListOptions options)
    {
        var query = options.ToQuery();
        query.Remove("_cursor");
        if (options.Limit is { } limit)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, 50);
            query["_limit"] = limit.ToString(CultureInfo.InvariantCulture);
        }
        if (options.ObjectType is { } type) query["object_type"] = type;
        if (options.Action is { } action) query["action"] = action;
        if (options.UpdatedSince is { } date) query["date_updated__gte"] = date.ToString("O", CultureInfo.InvariantCulture);
        return query;
    }
}
