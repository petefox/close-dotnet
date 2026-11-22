namespace Close.DotNet.Tests;

public sealed class LiveTests
{
    private static string? ApiKey => Environment.GetEnvironmentVariable("CLOSE_DEV_API_KEY");
    private static string? Organization => Environment.GetEnvironmentVariable("CLOSE_DEV_ORGANIZATION_ID");

    public static bool Enabled => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Organization);

    [Fact(Skip = "Requires an opted-in Close dev organization.", SkipUnless = nameof(Enabled))]
    public async Task DevOrganizationAllowsReadOnlyAuthentication()
    {
        var me = await new CloseClient(ApiKey!).Users.GetMeAsync(TestContext.Current.CancellationToken);
        Assert.StartsWith("user_", me.Id);
        Assert.Contains(me.Organizations ?? [], org => org.Id == Organization);
    }
}
