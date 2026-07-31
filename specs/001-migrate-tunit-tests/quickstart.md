# Quickstart: Validate the Unit-Test TUnit Migration

Run this guide after implementation. It proves the unit workflow defined in [contracts/test-execution.md](contracts/test-execution.md); it does not run or validate the deferred integration project.

## 1. Verify .NET prerequisites

```powershell
dotnet --info
dotnet --list-runtimes
```

Expected:

- The repository-selected .NET 10 SDK is active.
- Runtimes needed by the unit project's `net8.0`, `net9.0`, and `net10.0` targets are available as required by the host workflow.
- No Docker or external-service prerequisite is needed for this feature's validation.

## 2. Restore and build the unit project

```powershell
dotnet restore test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj
dotnet build test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-restore
```

Expected: every declared unit target builds without an xUnit runner or framework dependency.

## 3. Inspect unit discovery and run a focused case

Use the verified TUnit/Microsoft Testing Platform list option, then select a stable class with a tree-node filter:

```powershell
dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-build --list-tests
dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release -f net8.0 --no-build --treenode-filter "/*/*/OpenSearchJsonFormatterTests/*"
```

Expected:

- Unit discovery reconciles with the recorded baseline per target framework.
- Parameterized unit cases are separately attributable.
- The focused command runs 13 formatter cases and excludes the other 51 cases for the selected target.

To debug through the test executable without an IDE runner:

```powershell
dotnet run --project test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Debug -f net8.0 -- --treenode-filter "/*/*/OpenSearchJsonFormatterTests/*"
```

## 4. Run unit tests with results and coverage

```powershell
dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release -f net8.0 --no-build --report-trx --results-directory artifacts/test-results/unit/net8.0 --coverage --coverage-output-format cobertura --coverage-output "$PWD/artifacts/coverage/unit/net8.0.cobertura.xml"
```

Repeat the command with `net9.0` and `net10.0`, changing both artifact paths for each target. Run the complete multi-target suite without artifact switches with:

```powershell
dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-build
```

Expected:

- All unit targets execute.
- TRX and Cobertura artifacts exist in unit-specific locations.
- Pass/fail/skip outcomes reconcile with the approved unit baseline.

## 5. Audit scoped legacy-framework removal

Inspect the migrated unit project directly:

```powershell
rg -n -i "xunit|dotnet-xunit|IClassFixture|\[Fact|\[Theory|\[InlineData|\[MemberData" test/Serilog.Sinks.OpenSearch.Tests .github global.json --glob '!**/bin/**' --glob '!**/obj/**'
```

Expected: no active xUnit package, source, configuration, attribute, runner, or unit command remains in the scoped paths.

A separate repository-wide search will still find xUnit references under `test/Serilog.Sinks.OpenSearch.IntegrationTests`. Those are expected deferred-scope exceptions and must not be removed as part of this feature.

## 6. Prove repeatability

Run the complete unit command three consecutive times on a stable host. For each run, record the Unit Test Run Evidence fields defined in [data-model.md](data-model.md), especially discovery counts, outcomes, and artifact paths.

Expected: all three unit runs have consistent discovery and outcomes, excluding only intentionally skipped cases already in the baseline.

## 7. Verify migration boundaries

```powershell
git diff -- src
git diff -- test/Serilog.Sinks.OpenSearch.IntegrationTests
git diff --check
```

Compare these results with the scoped diff baseline captured before implementation.

Expected:

- No feature-driven production-source behavior changes are present.
- The integration-project source, project, configuration, and execution state matches its pre-implementation baseline, including preservation of any user changes that already existed.
- No whitespace errors are introduced.
- Any approved exception to unit discovery or outcomes is documented before completion.
