namespace Close.DotNet;

/// <summary>Transport settings captured when a <see cref="CloseClient"/> is created.</summary>
public sealed class CloseClientOptions
{
    /// <summary>Base address, including the API version. Use HTTPS except for loopback test servers.</summary>
    public Uri BaseAddress { get; init; } = new("https://api.close.com/api/v1/");

    /// <summary>Maximum read retries after the first attempt. Set to zero to disable SDK retries.</summary>
    public int MaxReadRetries { get; init; } = 2;

    /// <summary>Maximum total time the SDK may spend waiting between attempts.</summary>
    public TimeSpan RetryDelayBudget { get; init; } = TimeSpan.FromSeconds(15);
}

/// <summary>Filters and field projections for one request.</summary>
public class CloseRequestOptions
{
    /// <summary>Close field names to return. Missing response fields deserialize to null.</summary>
    public IReadOnlyList<string>? Fields { get; init; }

    /// <summary>Additional Close query parameters, with unescaped names and values.</summary>
    public IReadOnlyDictionary<string, string?>? Query { get; init; }

    internal Dictionary<string, string?> ToQuery()
    {
        var query = Query is null ? [] : new Dictionary<string, string?>(Query, StringComparer.Ordinal);
        if (Fields is { Count: > 0 }) query["_fields"] = string.Join(',', Fields);
        return query;
    }
}

/// <summary>Filters and starting position for an offset-paginated list.</summary>
public sealed class CloseListOptions : CloseRequestOptions
{
    /// <summary>Requested page size. Close enforces resource-specific limits.</summary>
    public int? Limit { get; init; }

    /// <summary>Starting offset.</summary>
    public int Skip { get; init; }
}
