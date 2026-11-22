using System.Net;
using System.Text.Json;

namespace Close.DotNet;

/// <summary>Thread-safe client for the Close CRM REST API.</summary>
public sealed partial class CloseClient
{
    private static readonly HttpClient SharedHttpClient = new(CreateHandler());
    private readonly CloseTransport transport;

    /// <summary>Creates a client with an API key and the shared HTTP connection pool.</summary>
    public CloseClient(string apiKey, CloseClientOptions? options = null)
        : this(CloseCredential.ApiKey(apiKey), SharedHttpClient, options) { }

    /// <summary>Creates a client with a credential and the shared HTTP connection pool.</summary>
    public CloseClient(CloseCredential credential, CloseClientOptions? options = null)
        : this(credential, SharedHttpClient, options) { }

    /// <summary>Creates a client using caller-owned HTTP transport.</summary>
    /// <remarks>Disable automatic redirects and configure timeouts on the supplied client. This SDK does not dispose it.</remarks>
    public CloseClient(CloseCredential credential, HttpClient httpClient, CloseClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(credential);
        ArgumentNullException.ThrowIfNull(httpClient);
        transport = new(httpClient, credential, options ?? new());
        Leads = new(transport);
        Contacts = new(transport);
        Opportunities = new(transport);
        Tasks = new(transport);
        Notes = new(transport);
        Events = new(transport);
        Webhooks = new(transport);
        Search = new(transport);
        Users = new(transport);
    }

    /// <summary>Companies and other lead records.</summary>
    public LeadsClient Leads { get; }

    /// <summary>People associated with leads.</summary>
    public ContactsClient Contacts { get; }

    /// <summary>Deals in lead pipelines.</summary>
    public OpportunitiesClient Opportunities { get; }

    /// <summary>Action items assigned to users.</summary>
    public TasksClient Tasks { get; }

    /// <summary>Note activities recorded against leads.</summary>
    public NotesClient Notes { get; }

    /// <summary>Change history from the event log.</summary>
    public EventsClient Events { get; }

    /// <summary>Webhook subscription management.</summary>
    public WebhooksClient Webhooks { get; }

    /// <summary>Advanced search across Close object types.</summary>
    public SearchClient Search { get; }

    /// <summary>User operations, including the authenticated user.</summary>
    public UsersClient Users { get; }

    /// <summary>Sends JSON to a relative Close API route. Supply unescaped query values separately.</summary>
    /// <remarks>The response data has an undefined JSON value when the server returns no content.</remarks>
    public async Task<CloseResponse<JsonElement>> SendAsync(
        HttpMethod method, string relativePath, IReadOnlyDictionary<string, string?>? query = null,
        JsonElement? body = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        var bytes = body is { } value ? JsonSerializer.SerializeToUtf8Bytes(value, CloseJson.Default.JsonElement) : null;
        return await transport.SendAsync(method, relativePath, query, bytes, CloseJson.Default.JsonElement,
            method == HttpMethod.Get || method == HttpMethod.Head, cancellationToken).ConfigureAwait(false);
    }

    internal static SocketsHttpHandler CreateHandler() => new()
    {
        AllowAutoRedirect = false,
        UseCookies = false,
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        AutomaticDecompression = DecompressionMethods.All
    };
}
