# Quickstart: Validate Trace and Span Context Formatting

## Prerequisites

- Run from the repository root using the SDK selected by `global.json`.
- Do not invoke `serilog-sinks-opensearch.sln` or anything under `test/Serilog.Sinks.OpenSearch.IntegrationTests/**`.

## 1. Restore scoped projects

```powershell
dotnet restore src/Serilog.Formatting.OpenSearch/Serilog.Formatting.OpenSearch.csproj
dotnet restore src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj
dotnet restore test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj
dotnet restore sample/Serilog.Sinks.OpenSearch.Sample/Serilog.Sinks.OpenSearch.Sample.csproj
```

Expected: all succeed without evaluating the integration project.

## 2. Build all in-scope consumers

```powershell
dotnet build src/Serilog.Formatting.OpenSearch/Serilog.Formatting.OpenSearch.csproj -c Release --no-restore
dotnet build src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj -c Release --no-restore
dotnet build test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-restore -m:1
dotnet build sample/Serilog.Sinks.OpenSearch.Sample/Serilog.Sinks.OpenSearch.Sample.csproj -c Release --no-restore -m:1
```

Expected: all pass. Serial MSBuild avoids the workstation's observed nested multi-target evaluation failure.

## 3. Run focused contracts on every target

```powershell
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net8.0/Serilog.Sinks.OpenSearch.Tests.exe --treenode-filter "/*/*/TraceContext*Tests/*"
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net9.0/Serilog.Sinks.OpenSearch.Tests.exe --treenode-filter "/*/*/TraceContext*Tests/*"
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net10.0/Serilog.Sinks.OpenSearch.Tests.exe --treenode-filter "/*/*/TraceContext*Tests/*"
```

Expected coverage: activity capture inside/outside; fixed/default IDs; exact formats; all opt-out combinations; inline/nested and casing/structured collisions; rendering metadata; fragment/delimiter behavior; derived formatter; custom/compact ownership; 10,000-event concurrency.

## 4. Run the complete suite

```powershell
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net8.0/Serilog.Sinks.OpenSearch.Tests.exe
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net9.0/Serilog.Sinks.OpenSearch.Tests.exe
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net10.0/Serilog.Sinks.OpenSearch.Tests.exe
```

Expected: all tests pass with only previously documented skips. Record discovery/pass/fail/skip counts.

## 5. Run controlled performance evidence

```powershell
& test/Serilog.Sinks.OpenSearch.Tests/bin/Release/net10.0/Serilog.Sinks.OpenSearch.Tests.exe --treenode-filter "/*/*/TraceContextFormatterBenchmarkTests/*"
```

Expected: after warm-up/interleaved trials over prebuilt events, median traced throughput is at least 95% of untraced. Run on an idle machine and record environment/raw samples; do not infer this from ordinary unit duration.

## 6. Pack and audit

```powershell
dotnet pack src/Serilog.Formatting.OpenSearch/Serilog.Formatting.OpenSearch.csproj -c Release --no-build -o artifacts/packages
dotnet pack src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj -c Release --no-build -o artifacts/packages
rg -n "TraceId|SpanId|renderTraceId|renderSpanId" src test/Serilog.Sinks.OpenSearch.Tests README.md CHANGES.md sample --glob "!test/Serilog.Sinks.OpenSearch.IntegrationTests/**"
git diff --check -- . ":(exclude)test/Serilog.Sinks.OpenSearch.IntegrationTests/**"
git status --short
```

Inspect `.nuspec` files: formatter is v3, the coordinated sink package uses its next major version, and the sink resolves formatter v3. Confirm existing and new constructor/configuration calls compile, alternate paths contain no rewriting, and the integration boundary remains user-owned.

See [trace-context-formatting.md](contracts/trace-context-formatting.md) and [data-model.md](data-model.md).
