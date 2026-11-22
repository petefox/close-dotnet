using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Close.DotNet;
using Microsoft.AspNetCore.DataProtection;

internal sealed class SampleStore
{
    private readonly string connections;
    private readonly string inbox;
    private readonly IDataProtector tokenProtector;
    private readonly IDataProtector eventProtector;
    private readonly ConcurrentDictionary<string, DateTimeOffset> pendingStates = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> connectionLocks = new(StringComparer.Ordinal);

    internal SampleStore(string directory, IDataProtectionProvider dataProtection)
    {
        connections = Directory.CreateDirectory(Path.Combine(directory, "connections")).FullName;
        inbox = Directory.CreateDirectory(Path.Combine(directory, "inbox")).FullName;
        tokenProtector = dataProtection.CreateProtector("Close.DotNet.Sample.Connections.v1");
        eventProtector = dataProtection.CreateProtector("Close.DotNet.Sample.Webhooks.v1");
    }

    internal void StartAuthorization(string state)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var expired in pendingStates.Where(entry => entry.Value <= now))
            pendingStates.TryRemove(expired.Key, out _);
        pendingStates[state] = now.AddMinutes(10);
    }

    internal bool ConsumeAuthorization(string state) =>
        pendingStates.TryRemove(state, out var expiry) && expiry > DateTimeOffset.UtcNow;

    internal Task SaveConnectionAsync(string id, CloseOAuthTokens tokens) =>
        SaveAsync(ConnectionPath(id), tokens, DateTimeOffset.UtcNow);

    internal async ValueTask<string> GetAccessTokenAsync(string id, CloseOAuthClient oauth, CancellationToken token)
    {
        var path = ConnectionPath(id);
        if (!File.Exists(path)) throw new ConnectionUnavailableException();

        var gate = connectionLocks.GetOrAdd(id, _ => new(1));
        await gate.WaitAsync(token);
        try
        {
            var encrypted = await File.ReadAllBytesAsync(path, token);
            var stored = JsonSerializer.Deserialize<TokenRecord>(tokenProtector.Unprotect(encrypted))
                ?? throw new ConnectionUnavailableException();
            if (stored.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1)) return stored.AccessToken;
            if (stored.RefreshToken is null) throw new ConnectionUnavailableException();

            var now = DateTimeOffset.UtcNow;
            using var refreshTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            CloseOAuthTokens replacement;
            try
            {
                replacement = await oauth.RefreshAsync(stored.RefreshToken, refreshTimeout.Token);
            }
            catch (CloseApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                exception.Details is { ValueKind: JsonValueKind.Object } details &&
                details.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String &&
                error.GetString() == "invalid_grant")
            {
                throw new ConnectionUnavailableException();
            }
            if (replacement.RefreshToken is null) throw new ConnectionUnavailableException();

            await SaveAsync(path, replacement, now);
            return replacement.AccessToken;
        }
        finally
        {
            gate.Release();
        }
    }

    internal Task EnqueueEventAsync(string eventId, byte[] payload, CancellationToken token)
    {
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(eventId)));
        var path = Path.Combine(inbox, key + ".event");
        return File.Exists(path)
            ? Task.CompletedTask
            : WriteAtomicallyAsync(path, eventProtector.Protect(payload), overwrite: false, token);
    }

    private Task SaveAsync(string path, CloseOAuthTokens tokens, DateTimeOffset issuedAt)
    {
        var record = new TokenRecord(tokens.AccessToken, tokens.RefreshToken, issuedAt.AddSeconds(tokens.ExpiresIn));
        var encrypted = tokenProtector.Protect(JsonSerializer.SerializeToUtf8Bytes(record));
        return WriteAtomicallyAsync(path, encrypted, overwrite: true, CancellationToken.None);
    }

    private string ConnectionPath(string id)
    {
        if (id.Length != 64 || !id.All(Uri.IsHexDigit)) throw new ConnectionUnavailableException();
        return Path.Combine(connections, id + ".json");
    }

    private static async Task WriteAtomicallyAsync(string path, byte[] bytes, bool overwrite, CancellationToken token)
    {
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllBytesAsync(temporary, bytes, token);
            try
            {
                File.Move(temporary, path, overwrite);
            }
            catch (IOException) when (!overwrite && File.Exists(path))
            {
                // Another delivery already persisted this event.
            }
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    private sealed record TokenRecord(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt);
}

internal sealed class ConnectionUnavailableException : Exception;
