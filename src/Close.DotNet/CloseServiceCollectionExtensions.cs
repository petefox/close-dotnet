using Microsoft.Extensions.DependencyInjection;

namespace Close.DotNet;

/// <summary>Registers the Close SDK with the standard HTTP client factory.</summary>
public static class CloseServiceCollectionExtensions
{
    /// <summary>Registers a Close client using an API key.</summary>
    public static IHttpClientBuilder AddCloseClient(
        this IServiceCollection services, string apiKey, CloseClientOptions? options = null)
    {
        var credential = CloseCredential.ApiKey(apiKey);
        return services.AddCloseClient(_ => credential, options);
    }

    /// <summary>Registers a Close client with a credential resolved from the caller's DI scope.</summary>
    public static IHttpClientBuilder AddCloseClient(
        this IServiceCollection services, Func<IServiceProvider, CloseCredential> credential, CloseClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(credential);
        return services.AddHttpClient("Close.DotNet")
            .ConfigurePrimaryHttpMessageHandler(CloseClient.CreateHandler)
            .RemoveAllLoggers()
            .AddTypedClient((http, provider) => new CloseClient(credential(provider), http, options));
    }
}
