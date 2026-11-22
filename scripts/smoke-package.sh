#!/usr/bin/env bash
set -euo pipefail

version="${1:?Pass the exact package version}"
source="${2:?Pass a package directory or the NuGet.org service index}"
shift 2
if [ "$#" -eq 0 ]; then
  set -- net10.0 net11.0
fi

feed="https://api.nuget.org/v3/index.json"
if [[ "$source" != "$feed" ]]; then
  source="$(cd "$source" && pwd)"
fi
scratch="$(mktemp -d)"
trap 'rm -rf "$scratch"' EXIT
cd "$scratch"
export NUGET_PACKAGES="$scratch/packages"

cat > Consumer.csproj <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>$(IFS=';'; echo "$*")</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Close.DotNet" Version="[$version]" />
  </ItemGroup>
</Project>
EOF

if [[ "$source" == "$feed" ]]; then
  package_sources="<clear /><add key=\"nuget.org\" value=\"$feed\" />"
  source_mapping=""
else
  mkdir feed
  cp "$source/Close.DotNet.$version.nupkg" feed/
  package_sources="<clear /><add key=\"package-under-test\" value=\"feed\" /><add key=\"nuget.org\" value=\"$feed\" />"
  source_mapping='<packageSourceMapping>
    <packageSource key="package-under-test"><package pattern="Close.DotNet" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>'
fi

cat > NuGet.Config <<EOF
<configuration>
  <packageSources>$package_sources</packageSources>
  $source_mapping
</configuration>
EOF

cat > Program.cs <<'EOF'
using System.Net;
using System.Runtime.InteropServices;
using Close.DotNet;

using var http = new HttpClient(new Fixture());
var sdk = new CloseClient(CloseCredential.ApiKey("package_test"), http);
var me = await sdk.Users.GetMeAsync();
if (me.Id != "user_package")
    throw new InvalidOperationException("The packaged SDK did not deserialize a user.");
Console.WriteLine($"Package smoke test passed on {RuntimeInformation.FrameworkDescription}.");

sealed class Fixture : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get || request.RequestUri?.AbsolutePath != "/api/v1/me/" ||
            request.Headers.Authorization?.Scheme != "Basic")
            throw new InvalidOperationException("Unexpected packaged SDK request.");

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":"user_package"}""")
        });
    }
}
EOF

dotnet restore --configfile NuGet.Config
for framework in "$@"; do
  dotnet run --framework "$framework" --configuration Release --no-restore
done
