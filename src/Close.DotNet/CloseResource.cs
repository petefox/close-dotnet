using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Close.DotNet;

/// <summary>Standard list, get, create, update, and delete operations for a Close resource.</summary>
/// <typeparam name="T">Response object.</typeparam>
/// <typeparam name="TCreate">Create request.</typeparam>
/// <typeparam name="TUpdate">Update request.</typeparam>
public class CloseResource<T, TCreate, TUpdate> where T : CloseObject
    where TCreate : CloseWriteRequest where TUpdate : CloseWriteRequest
{
    private readonly CloseTransport transport;
    private readonly string path;

    private protected CloseTransport Transport => transport;

    internal CloseResource(CloseTransport transport, string path)
    {
        this.transport = transport;
        this.path = path;
    }

    /// <summary>Retrieves a single record by ID.</summary>
    public Task<T> GetAsync(string id, CloseRequestOptions? options = null, CancellationToken cancellationToken = default) =>
        ReadAsync(HttpMethod.Get, CloseRoutes.Item(path, id), options, null, cancellationToken);

    /// <summary>Creates a record.</summary>
    public Task<T> CreateAsync(TCreate request, CloseRequestOptions? options = null, CancellationToken cancellationToken = default) =>
        ReadAsync(HttpMethod.Post, path, options, Serialize(request), cancellationToken);

    /// <summary>Updates only fields explicitly supplied in the request.</summary>
    public Task<T> UpdateAsync(string id, TUpdate request, CloseRequestOptions? options = null, CancellationToken cancellationToken = default) =>
        ReadAsync(HttpMethod.Put, CloseRoutes.Item(path, id), options, Serialize(request), cancellationToken);

    /// <summary>Deletes a record by ID.</summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await transport.SendAsync<T>(HttpMethod.Delete, CloseRoutes.Item(path, id), null, null, null, false, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Retrieves one page. Use the returned data count to advance the offset.</summary>
    public Task<ClosePage<T>> ListPageAsync(CloseListOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new();
        return ReadPageAsync(options.ToQuery(), options.Skip, options.Limit, cancellationToken);
    }

    /// <summary>Enumerates list results, fetching a page only when needed.</summary>
    public IAsyncEnumerable<T> ListAsync(CloseListOptions? options = null, CancellationToken cancellationToken = default) =>
        CloseRoutes.StreamOffsetAsync(options ?? new(), ReadPageAsync, cancellationToken);

    private async Task<T> ReadAsync(HttpMethod method, string route, CloseRequestOptions? options, byte[]? body, CancellationToken token) =>
        (await transport.SendAsync(method, route, options?.ToQuery(), body, CloseJson.Type<T>(), method == HttpMethod.Get, token)
            .ConfigureAwait(false)).RequireData();

    private async Task<ClosePage<T>> ReadPageAsync(Dictionary<string, string?> query, int skip, int? limit, CancellationToken token)
    {
        CloseRoutes.SetOffset(query, skip, limit);
        var page = (await transport.SendAsync(HttpMethod.Get, path, query, null, CloseJson.Type<ClosePage<T>>(), true, token)
            .ConfigureAwait(false)).RequireData();
        return page.Data is not null ? page : throw new InvalidOperationException("Close returned a null list.");
    }

    private static byte[] Serialize<TBody>(TBody request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return JsonSerializer.SerializeToUtf8Bytes(request, CloseJson.Type<TBody>());
    }
}

/// <summary>Lead records.</summary>
public sealed partial class LeadsClient : CloseResource<Lead, CreateLeadRequest, UpdateLeadRequest>
{
    internal LeadsClient(CloseTransport transport) : base(transport, "lead/") { }
}

/// <summary>Contacts on leads.</summary>
public sealed class ContactsClient : CloseResource<Contact, CreateContactRequest, UpdateContactRequest>
{
    internal ContactsClient(CloseTransport transport) : base(transport, "contact/") { }
}

/// <summary>Pipeline opportunities.</summary>
public sealed class OpportunitiesClient : CloseResource<Opportunity, CreateOpportunityRequest, UpdateOpportunityRequest>
{
    internal OpportunitiesClient(CloseTransport transport) : base(transport, "opportunity/") { }
}

/// <summary>Tasks on leads and contacts.</summary>
public sealed partial class TasksClient : CloseResource<CloseTask, CreateTaskRequest, UpdateTaskRequest>
{
    internal TasksClient(CloseTransport transport) : base(transport, "task/") { }
}

/// <summary>Notes on leads.</summary>
public sealed class NotesClient : CloseResource<Note, CreateNoteRequest, UpdateNoteRequest>
{
    internal NotesClient(CloseTransport transport) : base(transport, "activity/note/") { }
}

/// <summary>Webhook subscriptions for the authenticated organization.</summary>
public sealed class WebhooksClient : CloseResource<WebhookSubscription, CreateWebhookRequest, UpdateWebhookRequest>
{
    internal WebhooksClient(CloseTransport transport) : base(transport, "webhook/") { }
}

/// <summary>User endpoints.</summary>
public sealed partial class UsersClient
{
    private readonly CloseTransport transport;

    internal UsersClient(CloseTransport transport) => this.transport = transport;

    /// <summary>Retrieves the authenticated user.</summary>
    public async Task<CloseUser> GetMeAsync(CancellationToken cancellationToken = default) =>
        (await transport.SendAsync(HttpMethod.Get, "me/", null, null, CloseJson.Type<CloseUser>(), true, cancellationToken)
            .ConfigureAwait(false)).RequireData();
}

internal static class CloseRoutes
{
    internal static string Item(string path, string id) => path + Segment(id) + "/";

    internal static string Segment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value is "." or ".." || value.IndexOfAny(['/', '\\', '%']) >= 0 || value.Any(char.IsControl))
            throw new ArgumentException("The path identifier must be one segment.", nameof(value));
        return Uri.EscapeDataString(value);
    }

    internal static T RequireData<T>(this CloseResponse<T?> response) where T : class =>
        response.Data ?? throw new CloseApiException(response.StatusCode, response.RequestId, response.RateLimit,
            responseProblem: "Close returned an empty response where JSON was expected.");

    internal static void SetOffset(Dictionary<string, string?> query, int skip, int? limit)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        if (limit is { } size) ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        query["_skip"] = skip.ToString(CultureInfo.InvariantCulture);
        if (limit is { } pageSize) query["_limit"] = pageSize.ToString(CultureInfo.InvariantCulture);
    }

    internal static async IAsyncEnumerable<T> StreamOffsetAsync<T>(
        CloseListOptions options, Func<Dictionary<string, string?>, int, int?, CancellationToken, Task<ClosePage<T>>> fetch,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = options.ToQuery();
        var skip = options.Skip;
        while (true)
        {
            var page = await fetch(query, skip, options.Limit, cancellationToken).ConfigureAwait(false);
            foreach (var item in page.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }

            if (!page.HasMore) yield break;
            if (page.Data.Count == 0)
                throw new InvalidOperationException("Close reported more items without advancing the offset.");
            skip = checked(skip + page.Data.Count);
        }
    }

    internal static async IAsyncEnumerable<T> StreamCursorAsync<T>(
        string? cursor, Func<string?, CancellationToken, Task<(IReadOnlyList<T> Data, string? Next)>> fetch,
        [EnumeratorCancellation] CancellationToken token)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (!string.IsNullOrEmpty(cursor)) seen.Add(cursor);
        while (true)
        {
            var page = await fetch(cursor, token).ConfigureAwait(false);
            foreach (var item in page.Data)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
            if (string.IsNullOrEmpty(page.Next)) yield break;
            if (!seen.Add(page.Next))
                throw new InvalidOperationException("Close returned a previously visited cursor.");
            cursor = page.Next;
        }
    }
}
