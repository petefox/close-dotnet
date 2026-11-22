# Contributing

Install the .NET SDK specified in [global.json](global.json), then run:

~~~bash
dotnet restore --locked-mode
dotnet build Close.DotNet.slnx --configuration Release --no-restore
dotnet test --solution Close.DotNet.slnx --configuration Release --no-build
~~~

The test suite uses synthetic HTTP responses and a local loopback server. The optional live test runs a read-only identity request when both `CLOSE_DEV_API_KEY` and `CLOSE_DEV_ORGANIZATION_ID` are set for a dedicated Close development organization.

For a new endpoint, link the corresponding Close reference, add a request contract test, and update [the coverage inventory](docs/endpoint-coverage.csv). Run `python3 scripts/coverage.py --write` to regenerate the table and `python3 scripts/coverage.py --upstream` to reconcile against Close's current reference.

For generated resource clients, fetch the public specification and regenerate from the coverage inventory.

~~~bash
curl -fsSL https://api.close.com/api/openapi.json -o /tmp/close-openapi.json
python3 scripts/generate-endpoints.py /tmp/close-openapi.json
python3 scripts/coverage.py --write --upstream --stable
~~~

Review generated wire types against the linked reference. The generator uses published shapes and examples. Use an explicit model or a manual operation for endpoints whose runtime behavior needs specialized handling.

Build the docs after the Release build with `dotnet tool restore` and `dotnet docfx docs/docfx.json --warningsAsErrors`. The rendered site is in `docs/_site`.

Use a GitHub issue for bugs or requests. Include the SDK version and a small, redacted reproduction with the relevant method and status code. Use [private reporting](SECURITY.md) for vulnerabilities.
