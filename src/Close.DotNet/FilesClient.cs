using System.Net;
using System.Text.Json;

namespace Close.DotNet;

/// <summary>Creates signed upload tickets and streams attachments to Close's storage provider.</summary>
public sealed class FilesClient
{
    private readonly CloseTransport transport;
    private readonly StorageTransfer storage;

    internal FilesClient(CloseTransport transport) : this(transport, StorageTransfer.Shared) { }

    internal FilesClient(CloseTransport transport, StorageTransfer storage)
    {
        this.transport = transport;
        this.storage = storage;
    }

    /// <summary>Creates a short-lived upload ticket for an attachment.</summary>
    public async Task<CloseUploadTicket> CreateUploadAsync(string filename, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        var mediaType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
        var request = new CloseUploadRequest(filename, mediaType.ToString());
        var body = JsonSerializer.SerializeToUtf8Bytes(request, CloseJson.Type<CloseUploadRequest>());
        return (await transport.SendAsync(HttpMethod.Post, "files/upload/", null, body,
            CloseJson.Type<CloseUploadTicket>(), false, cancellationToken).ConfigureAwait(false)).RequireData();
    }

    /// <summary>Creates a ticket, streams a file from its current position, and returns its temporary Close reference.</summary>
    /// <remarks>The caller retains ownership of the file stream. Use its returned URL promptly when calling the attachment API.</remarks>
    public async Task<CloseAttachment> UploadAsync(Stream file, string filename, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        if (!file.CanRead) throw new ArgumentException("The upload stream must be readable.", nameof(file));
        var mediaType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
        long? size = file.CanSeek ? file.Length - file.Position : null;

        var ticket = await CreateUploadAsync(filename, mediaType.ToString(), cancellationToken).ConfigureAwait(false);
        if (ticket.Upload is null || ticket.Upload.Url is null || ticket.Upload.Fields is null || ticket.Download?.Url is null)
            throw new InvalidOperationException("Close returned an incomplete upload ticket.");

        using var form = new MultipartFormDataContent();
        foreach (var (name, value) in ticket.Upload.Fields)
            form.Add(new StringContent(value), name);
        var content = new FileUploadContent(file);
        content.Headers.ContentType = mediaType;
        form.Add(content, "file", filename);
        await storage.UploadAsync(ticket.Upload.Url, form, cancellationToken).ConfigureAwait(false);
        return new(ticket.Download.Url.AbsoluteUri, filename, mediaType.ToString(), size);
    }

    private sealed class FileUploadContent(Stream file) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
            file.CopyToAsync(stream, cancellationToken);

        protected override bool TryComputeLength(out long length)
        {
            length = file.CanSeek ? file.Length - file.Position : 0;
            return file.CanSeek;
        }
    }
}

internal sealed record CloseUploadRequest(string Filename, string ContentType);

/// <summary>Short-lived parameters needed to finish uploading a Close attachment.</summary>
public sealed class CloseUploadTicket
{
    /// <summary>Destination and signed fields for the multipart request.</summary>
    public CloseUploadDestination? Upload { get; init; }

    /// <summary>URL to reference after the upload succeeds.</summary>
    public CloseTemporaryFile? Download { get; init; }
}

/// <summary>Signed multipart upload details issued by Close.</summary>
public sealed class CloseUploadDestination
{
    /// <summary>Upload URL supplied by Close.</summary>
    public Uri? Url { get; init; }

    /// <summary>Signed storage form fields. Keep these values private.</summary>
    public IReadOnlyDictionary<string, string>? Fields { get; init; }
}

/// <summary>Temporary file reference supplied by Close.</summary>
public sealed class CloseTemporaryFile
{
    /// <summary>URL used to reference the uploaded file in a Close API call.</summary>
    public Uri? Url { get; init; }
}

/// <summary>Provides streaming downloads for completed exports.</summary>
public sealed partial class ExportsClient
{
    /// <summary>Downloads a completed export into a caller-owned stream.</summary>
    public Task DownloadAsync(Models.Export export, Stream destination, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(export);
        if (!Uri.TryCreate(export.DownloadUrl, UriKind.Absolute, out var url))
            throw new InvalidOperationException("The export has no download URL. Retrieve the completed export to obtain one.");
        return StorageTransfer.Shared.DownloadAsync(url, destination, cancellationToken);
    }
}

internal sealed class StorageTransfer(HttpClient httpClient)
{
    internal static StorageTransfer Shared { get; } = new(new HttpClient(CloseClient.CreateHandler()));

    internal async Task UploadAsync(Uri url, HttpContent content, CancellationToken cancellationToken)
    {
        ValidateUrl(url);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(response);
    }

    internal async Task DownloadAsync(Uri url, Stream destination, CancellationToken cancellationToken)
    {
        ValidateUrl(url);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("The download stream must be writable.", nameof(destination));

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(response);
        await response.Content.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateUrl(Uri url)
    {
        if (!url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps || url.UserInfo.Length != 0 || url.Fragment.Length != 0)
            throw new ArgumentException("Close storage URLs must use HTTPS and may not contain user information or fragments.", nameof(url));
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Close file transfer returned HTTP {(int)response.StatusCode}.", null, response.StatusCode);
    }
}
