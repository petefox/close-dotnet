# Release Close.DotNet

1. Update the package version and changelog on `main`. Wait for the CI run on the release commit to pass.
2. Create and push an annotated `v<version>` tag on that commit. Run **GitHub release** from `main` with that tag. The workflow reconciles coverage, builds and tests the tagged commit, and publishes its assets as an immutable release. In repository settings, enable release immutability before the first release.
3. For the first NuGet release, sign into NuGet.org as the package owner. Create a GitHub Actions Trusted Publishing policy for repository owner `petefox`, repository `close-dotnet`, workflow `publish-nuget.yml`, environment `nuget`, with new package and new version scope restricted to `Close.DotNet`. In that GitHub environment set the secret `NUGET_USER` to the NuGet.org profile name.
4. Run **Publish to NuGet** from `main` with the same tag. The workflow verifies and pushes the existing artifact and installs the exact version in clean .NET 10 and 11 consumers.
5. Verify the GitHub release, the NuGet listing and symbols, and the public documentation site. Correct a released package by issuing a new version.

[NuGet Trusted Publishing setup](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) · [GitHub immutable releases](https://docs.github.com/en/code-security/how-tos/secure-your-supply-chain/establish-provenance-and-integrity/prevent-release-changes)
