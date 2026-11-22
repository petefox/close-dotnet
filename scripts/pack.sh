#!/usr/bin/env bash
set -euo pipefail

destination="${1:?Pass the package output directory}"
mkdir -p "$destination"
destination="$(cd "$destination" && pwd)"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
timestamp="$(git -C "$root" show -s --format=%ct HEAD)"
scratch="$(mktemp -d)"
trap 'rm -rf "$scratch"' EXIT
cd "$scratch"

# .NET 11 writes deterministic NuGet archives. The assembly is still built with the repository's .NET 10 SDK.
cat > global.json <<'JSON'
{
  "sdk": { "version": "11.0.100", "rollForward": "latestFeature", "allowPrerelease": false }
}
JSON
dotnet restore "$root/src/Close.DotNet" --locked-mode --configfile "$root/NuGet.Config"
# RepositoryUrl is explicit in the project. Skip the checkout-specific branch attribute.
dotnet pack "$root/src/Close.DotNet" --configuration Release --no-build \
  -p:DeterministicTimestamp="$timestamp" -p:PublishRepositoryUrl=false --output "$destination"
