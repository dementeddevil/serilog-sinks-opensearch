# Unit-Test Migration Inventory

**Captured**: 2026-07-31 before unit-project package or source conversion

## Source Inventory

| Measure | Baseline |
|---|---:|
| C# test/support files | 35 |
| Active annotated test methods | 50 |
| Fact methods | 45 |
| Theory methods | 5 |
| Inline data rows | 16 |
| Member-data sources | 1 |
| Member-data rows | 3 |
| Source-derived expanded cases per target | 64 |
| Intentionally skipped cases per target | 5 |
| Target frameworks | `net8.0`, `net9.0`, `net10.0` |

The expanded-case expectation is `45` facts plus `6` FileSet rows, `6` OpenSearchPayloadReader rows, `4` formatter property rows, and `3` formatter exception rows.

All five skips are in `test/Serilog.Sinks.OpenSearch.Tests/BulkActionTests.cs` and use the reason `Flaky test on GitHub actions`.

## Executable Baseline

The initial restore succeeded after NuGet network access was allowed. A three-target `dotnet test --list-tests` attempt under the machine-default .NET 11 preview SDK did not complete within the bounded run and was terminated. A Release build under that preview SDK exited with code 1 after only `NETSDK1057`, reporting zero warnings and zero errors. The baseline was then retried with the repository pinned to the installed stable .NET 10 SDK before changing the unit framework packages or source. The Release build succeeded with the same pre-existing `CS0672` warning on each target and no errors.

| Target | Discovery | Outcomes |
|---|---|---|
| `net8.0` | 62 listed runner nodes; member data listed as one unexpanded node | Passed 59, skipped 5, failed 0, total 64 |
| `net9.0` | Source/runner parity from the same compiled suite; execution expanded all cases | Passed 59, skipped 5, failed 0, total 64 |
| `net10.0` | Source/runner parity from the same compiled suite; execution expanded all cases | Passed 59, skipped 5, failed 0, total 64 |

The xUnit list command was run explicitly on `net8.0` and returned 62 nodes because its three-row member data appeared as one node. Complete execution expanded that member data and proved 64 cases on all three targets.

## Active Legacy Framework Surface

### Unit project

`test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` owns:

- `Microsoft.NET.Test.Sdk` 18.8.1
- `xunit` 2.9.3
- `xunit.runner.visualstudio` 3.1.5
- `coverlet.msbuild` 10.0.1
- `ReportGenerator` 5.5.11
- Conditional `GitHubActionsTestLogger` 2.0.1
- `CollectCoverage`, `CoverletOutput`, `CoverletOutputFormat`, HTML report generation, and GitHub output targets

### Unit sources

- 29 files contain an xUnit import, attribute, assertion, collection declaration, or commented xUnit assertion.
- Active attributes are `[Fact]`, `[Theory]`, `[InlineData]`, `[MemberData]`, and two `[Collection("isolation")]` declarations.
- `test/Serilog.Sinks.OpenSearch.Tests/OpenSearchJsonFormatterTests.cs` owns all live `Assert.*` calls.
- `test/Serilog.Sinks.OpenSearch.Tests/FileSetTests.cs` and `OpenSearchPayloadReaderTests.cs` implement per-instance `IDisposable` cleanup.
- Commented-out tests in `OpenSearchSinkTests.cs` and `OpenSearchLogShipperTests.cs` are not active baseline cases and must remain commented.

### CI

`.github/workflows/cicd.yaml` runs solution-wide `dotnet test` with HTML, TRX, and GitHubActions loggers, uploads an HTML result, and feeds TRX files to `dorny/test-reporter`.

## Baseline Rules

- Compare discovery per target framework; do not combine the three target runs into one logical count.
- Preserve all five skip reasons.
- Treat the source-derived 64 cases as the fallback baseline only where the legacy runner cannot produce executable evidence.
- Record any post-migration difference explicitly rather than inferring parity from a successful build.
