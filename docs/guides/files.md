# Upload files and download exports

## Attach a file

`Files.UploadAsync` creates a signed ticket, uploads the bytes, and returns a temporary reference. The SDK reads from the stream's current position. Keep the stream open until the call returns.

~~~csharp
await using var source = File.OpenRead(path);
var attachment = await close.Files.UploadAsync(
    source, Path.GetFileName(path), "application/pdf", cancellationToken);

await close.Notes.CreateAsync(new()
{
    LeadId = leadId,
    Text = "Supporting document",
    Attachments = [attachment]
}, cancellationToken: cancellationToken);
~~~

Use the returned attachment promptly. Close's temporary download URLs expire. The upload goes through a dedicated storage client that keeps Close API credentials on the Close API request.

## Stream an export to disk

Retrieve the completed export by ID and pass a writable stream to `DownloadAsync`. If the job is still running, retrieve it again after it completes. Use a fresh job response when its signed URL expires.

~~~csharp
var export = await close.Exports.GetAsync(exportId, cancellationToken: cancellationToken);
await using var output = new FileStream(outputPath, FileMode.CreateNew);
await close.Exports.DownloadAsync(export, output, cancellationToken);
~~~

The SDK copies bytes directly into the destination and leaves it open for the caller. Keep signed URLs private.

[Close file uploads](https://developer.close.com/api/resources/files/) · [Close exports](https://developer.close.com/api/resources/exports)
