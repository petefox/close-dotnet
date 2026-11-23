using System.Globalization;

namespace Close.DotNet;

/// <summary>A Close filter value that accepts either a calendar date or a timestamp with an offset.</summary>
public readonly struct CloseFilterDate
{
    internal string? Value { get; }

    private CloseFilterDate(string value) => Value = value;

    /// <summary>Filters by calendar date.</summary>
    public static implicit operator CloseFilterDate(DateOnly date) => new(date.ToString("O", CultureInfo.InvariantCulture));

    /// <summary>Filters by timestamp, preserving the supplied offset.</summary>
    public static implicit operator CloseFilterDate(DateTimeOffset timestamp) => new(timestamp.ToString("O", CultureInfo.InvariantCulture));
}

internal sealed class CloseQueryParameters(IReadOnlyDictionary<string, string?>? fallback)
    : Dictionary<string, string?>(fallback ?? Empty, StringComparer.Ordinal)
{
    private static readonly Dictionary<string, string?> Empty = [];
    private Dictionary<string, IReadOnlyList<string>>? repeated;

    internal void Set(string name, string? value)
    {
        if (value is not null) this[name] = value;
    }

    internal void Set(string name, bool? value) => Set(name, value is { } flag ? (flag ? "true" : "false") : null);
    internal void Set(string name, long? value) => Set(name, value?.ToString(CultureInfo.InvariantCulture));
    internal void Set(string name, decimal? value) => Set(name, value?.ToString(CultureInfo.InvariantCulture));
    internal void Set(string name, DateOnly? value) => Set(name, value?.ToString("O", CultureInfo.InvariantCulture));
    internal void Set(string name, DateTimeOffset? value) => Set(name, value?.ToString("O", CultureInfo.InvariantCulture));
    internal void Set(string name, CloseFilterDate? value) => Set(name, value?.Value);

    internal void SetCsv(string name, IReadOnlyList<string>? values)
    {
        if (values is { Count: > 0 }) this[name] = string.Join(',', values);
    }

    internal void SetRepeated(string name, IReadOnlyList<string>? values)
    {
        if (values is not { Count: > 0 }) return;
        // Snapshot the sequence so a later page uses the same filters as the first page.
        var snapshot = values.ToArray();
        (repeated ??= new(StringComparer.Ordinal))[name] = snapshot;
        this[name] = snapshot[0];
    }

    internal static string Encode(IReadOnlyDictionary<string, string?> query)
    {
        var parameters = query as CloseQueryParameters;
        return string.Join('&', query.Where(entry => entry.Value is not null).SelectMany(entry =>
        {
            var prefix = Uri.EscapeDataString(entry.Key) + "=";
            return parameters?.repeated?.TryGetValue(entry.Key, out var values) == true
                ? values.Select(value => prefix + Uri.EscapeDataString(value))
                : [prefix + Uri.EscapeDataString(entry.Value!)];
        }));
    }
}
