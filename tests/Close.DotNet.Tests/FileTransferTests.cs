using System.Net;
using System.Text;
using System.Text.Json;

namespace Close.DotNet.Tests;

public sealed class FileTransferTests
{
    [Fact]
    public async Task UploadUsesTwoRequestsAndSendsSignedFieldsWithoutCloseCredentials()
    {
        var token = TestContext.Current.CancellationToken;
        using var closeHandler = new ScriptedHandler(_ => JsonResponse.Ok("""
            {"upload":{"url":"https://storage.example.org/bucket/","fields":{"key":"temporary/file","policy":"signed"}},
             "download":{"url":"https://app.close.com/go/file/temporary/test/archive.txt/"}}
            """));
        using var storageHandler = new ScriptedHandler(_ => new(HttpStatusCode.Created));
        using var closeHttp = new HttpClient(closeHandler);
        using var storageHttp = new HttpClient(storageHandler);
        var closeTransport = new CloseTransport(closeHttp, CloseCredential.ApiKey("test_key"), new());
        var files = new FilesClient(closeTransport, new StorageTransfer(storageHttp));
        using var file = new MemoryStream("skip:uploaded content"u8.ToArray());
        file.Position = 5;

        var reference = await files.UploadAsync(file, "archive.txt", "text/plain", token);

        var ticket = Assert.Single(closeHandler.Requests);
        Assert.Equal(HttpMethod.Post, ticket.Method);
        Assert.Equal("/api/v1/files/upload/", ticket.Uri.AbsolutePath);
        Assert.Equal("Basic dGVzdF9rZXk6", ticket.Authorization);
        using var ticketJson = JsonDocument.Parse(ticket.Body!);
        Assert.Equal("archive.txt", ticketJson.RootElement.GetProperty("filename").GetString());
        Assert.Equal("text/plain", ticketJson.RootElement.GetProperty("content_type").GetString());

        var upload = Assert.Single(storageHandler.Requests);
        Assert.Null(upload.Authorization);
        Assert.Equal("https://storage.example.org/bucket/", upload.Uri.AbsoluteUri);
        Assert.Equal("multipart/form-data", upload.MediaType);
        Assert.Contains("temporary/file", upload.Body);
        Assert.Contains("signed", upload.Body);
        Assert.Contains("filename=archive.txt", upload.Body);
        Assert.Contains("uploaded content", upload.Body);
        Assert.DoesNotContain("skip:", upload.Body);
        Assert.Equal(16L, reference.Size);
        Assert.EndsWith("/archive.txt/", reference.Url);
        Assert.True(file.CanRead);
    }

    [Fact]
    public async Task ExportDownloadPreservesSignedUrlAndLeavesTheDestinationOpen()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => new(HttpStatusCode.OK)
        {
            Content = new StringContent("id,name\nlead_example,Example\n")
        });
        using var http = new HttpClient(handler);
        using var destination = new MemoryStream();
        var storage = new StorageTransfer(http);

        await storage.DownloadAsync(new("https://storage.example.org/export.csv?signature=a%2Bb&expires=60"), destination, token);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Null(request.Authorization);
        Assert.Equal("?signature=a%2Bb&expires=60", request.Uri.Query);
        Assert.Equal("id,name\nlead_example,Example\n", Encoding.UTF8.GetString(destination.ToArray()));
        Assert.True(destination.CanWrite);
    }

    [Fact]
    public async Task ExportListDeserializesJobsAndChecksAvailabilityBeforeDownload()
    {
        var token = TestContext.Current.CancellationToken;
        using var handler = new ScriptedHandler(_ => JsonResponse.Ok("""
            {"data":[{"id":"expo_demo","status":"done","download_url":"https://storage.example.org/export.csv?signature=abc"}],
             "has_more":false}
            """));
        using var http = new HttpClient(handler);
        var exports = TestClient.Create(http).Exports;
        var response = await exports.ListPageAsync(cancellationToken: token);
        var export = Assert.Single(response.Data!);
        Assert.Equal("expo_demo", export.Id);
        Assert.Equal("done", export.Status);
        Assert.Equal("https://storage.example.org/export.csv?signature=abc", export.DownloadUrl);
        using var destination = new MemoryStream();
        await Assert.ThrowsAsync<InvalidOperationException>(() => exports.DownloadAsync(new() { Status = "running" }, destination, token));
    }
}
