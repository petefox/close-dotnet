# Connect an ASP.NET Core app with OAuth

[Register an OAuth app](https://developer.close.com/integrations/create-an-oauth-app) in your Close development organization and set its redirect URL to `https://localhost:7041/callback/close`. Close requires HTTPS, and the URL needs to match the sample configuration exactly.

Configure the [ASP.NET Core sample](https://github.com/petefox/close-dotnet/tree/main/samples/OAuthAndWebhooks) with .NET user secrets:

~~~bash
dotnet dev-certs https --trust
dotnet user-secrets --project samples/OAuthAndWebhooks set Close:ClientId YOUR_CLIENT_ID
dotnet user-secrets --project samples/OAuthAndWebhooks set Close:ClientSecret YOUR_CLIENT_SECRET
dotnet user-secrets --project samples/OAuthAndWebhooks set Close:RedirectUri https://localhost:7041/callback/close
dotnet run --project samples/OAuthAndWebhooks -- --urls https://localhost:7041
~~~

Open `https://localhost:7041/connect`, select the development organization, and consent. The app validates a short-lived, single-use OAuth state linked to an HTTPS-only browser cookie, exchanges the code, and calls `/me` to show the Close user ID.

The sample protects tokens with ASP.NET Core Data Protection and stores them on disk under `.local/`. It serializes refreshes for each connection, replaces the stored tokens atomically, and supplies the current access token using `CloseCredential.AccessTokenProvider`. For a multi-instance deployment, back the store and lock with shared infrastructure and protect the key ring at rest. Close invalidates the previous refresh token on rotation.

Call `CloseOAuthClient.RevokeAsync` with the active refresh token when disconnecting a production integration, then remove the stored connection.

[Close OAuth grant and token rotation](https://developer.close.com/api/overview/oauth-authentication)
