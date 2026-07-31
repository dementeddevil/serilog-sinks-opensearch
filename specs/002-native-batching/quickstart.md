# Quickstart: Validate Native Serilog Batching

## Prerequisites

- Run commands from the repository root.
- Use the .NET SDK selected by `global.json` (10.0.302 or its allowed patch roll-forward).
- Ensure required NuGet feeds are reachable for restore.
- Do not restore, build, test, or modify `test/Serilog.Sinks.OpenSearch.IntegrationTests`; it is deferred and contains user-owned changes.

## 1. Restore only in-scope projects

```powershell
dotnet restore src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj
dotnet restore test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj
dotnet restore sample/Serilog.Sinks.OpenSearch.Sample/Serilog.Sinks.OpenSearch.Sample.csproj
```

Expected: all three commands succeed without invoking the integration-test project.

## 2. Build production, unit, and consumer sample projects

```powershell
dotnet build src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj -c Release --no-restore
dotnet build test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-restore
dotnet build sample/Serilog.Sinks.OpenSearch.Sample/Serilog.Sinks.OpenSearch.Sample.csproj -c Release --no-restore
```

Expected: builds succeed with warnings treated according to each project, direct `OpenSearchSink` usage still compiles, and no compatibility-package type is required.

## 3. Run focused native-batching coverage

```powershell
dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release -f net8.0 --no-build --treenode-filter "/*/*/NativeBatching*Tests/*"
```

Expected coverage includes:

- option mapping, ten-minute native default, configured/zero/negative retry, and queue validation;
- whole-batch exception/unsuccessful-response propagation;
- retry then recovery without duplicates;
- temporary, permanent, and final listener events plus fallback routing;
- valid partial-bulk item handling through legacy handlers only;
- eager emission, batch partitioning, success reset, and disposal/final flushing;
- no assertion of an overflow notification that Serilog 4.4 does not emit.

## 4. Run the complete unit suite

```powershell
dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-build
```

Expected: .NET 8.0, .NET 9.0, and .NET 10.0 all pass with the five existing intentional skips per target retained. Reconcile the pre-feature 64 cases per target with the enumerated new native-batching cases.

Repeat the complete command three times in a stable environment. Discovery and outcomes must be identical across all three runs.

## 5. Pack and inspect dependencies

```powershell
dotnet pack src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj -c Release --no-build -o artifacts/packages
dotnet nuget why src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj Serilog.Sinks.PeriodicBatching
```

Expected: the package is created, its dependency metadata contains Serilog but not `Serilog.Sinks.PeriodicBatching`, and `dotnet nuget why` reports no dependency path for the compatibility package.

Inspect the generated `.nuspec` inside the `.nupkg` as the final package-metadata authority.

## 6. Run scoped audits

```powershell
rg -n "Serilog\.Sinks\.PeriodicBatching|PeriodicBatchingSink|PeriodicBatchingSinkOptions" src test/Serilog.Sinks.OpenSearch.Tests README.md CHANGES.md sample --glob "!test/Serilog.Sinks.OpenSearch.IntegrationTests/**"
git diff --check -- . ":(exclude)test/Serilog.Sinks.OpenSearch.IntegrationTests/**"
git status --short
```

Expected:

- no active compatibility API or package references remain in production, unit, sample, or current guidance;
- any historical changelog reference is explicitly classified rather than silently rewritten;
- no whitespace errors are introduced;
- existing integration-project changes remain untouched and are not attributed to this feature.

## Validation Evidence

Record:

1. Production, unit, and sample build results.
2. Focused native-batching case count and outcomes.
3. Three complete all-target unit counts and outcomes.
4. Packed dependency inspection.
5. Scoped compatibility-reference and whitespace audits.
6. Confirmation that the deferred integration directory received no feature-driven change.

See [native-batching.md](contracts/native-batching.md) for the behavioral contract and [data-model.md](data-model.md) for delivery/failure state transitions.
