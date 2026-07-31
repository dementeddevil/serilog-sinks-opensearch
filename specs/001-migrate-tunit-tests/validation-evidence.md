# TUnit Migration Validation Evidence

**Validated**: 2026-07-31  
**SDK**: .NET SDK 10.0.302 selected by `global.json`  
**Scope**: `test/Serilog.Sinks.OpenSearch.Tests` only; integration tests were not built, run, or migrated by this feature

## Restore and Build

| Check | Result |
|---|---|
| `dotnet restore test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` | Exit 0 after NuGet network access was allowed; all three project dependencies restored or were current |
| Release build `-f net8.0` | Exit 0; one pre-existing `CS0672` warning, zero errors |
| Release build `-f net9.0` | Exit 0; one pre-existing `CS0672` warning, zero errors |
| Release build `-f net10.0` | Exit 0; one pre-existing `CS0672` warning, zero errors |

No TUnit analyzer warning remains. The production projects compile as dependencies, but no file under `src/**` was changed.

## Discovery and Outcome Parity

`dotnet test <unit-project> -c Release --no-build --list-tests` exited 0 and reported 64 cases from each assembly, 192 total. This equals the source-derived pre-migration baseline and expands the three member-source rows individually.

| Target | Discovered | Passed | Skipped | Failed | Baseline difference |
|---|---:|---:|---:|---:|---|
| `net8.0` | 64 | 59 | 5 | 0 | None |
| `net9.0` | 64 | 59 | 5 | 0 | None |
| `net10.0` | 64 | 59 | 5 | 0 | None |

The five discoverable skips on each target are `DefaultBulkActionV7`, `BulkActionV7OverrideTypeName`, `DefaultBulkActionV8`, `PipelineAction`, and `BulkActionDataStreams`. Every skip retained `Flaky test on GitHub actions`.

The complete runs exercised the new per-test file cleanup and inherited connection cleanup without a lifecycle or analyzer failure. The two unavailable-server fixtures share `[NotInParallel("isolation")]`, preserving their keyed serialization while leaving unrelated tests parallel.

## Verified Runner Commands

- `--list-tests` discovered 64 cases per target.
- `-f net8.0`, `-f net9.0`, and `-f net10.0` each selected one target.
- `--treenode-filter "/*/*/OpenSearchJsonFormatterTests/*"` selected 13 formatter cases on `net8.0`, excluded the other 51 cases, and passed 13/13.
- `--report-trx` and `--results-directory` produced target-attributable TRX files.
- `--coverage --coverage-output-format cobertura --coverage-output <file>` produced target-attributable Cobertura files.
- `--report-trx-filename unit-net8.0.trx` was also verified for an explicit result filename.
- Runner switches are direct arguments to `dotnet test`; the documented `dotnet run --project` debug form places runner switches after `--`.

## Result and Coverage Artifacts

The instrumented per-target commands all exited 0 and produced:

| Target | TRX | Cobertura |
|---|---|---|
| `net8.0` | `artifacts/test-results/unit/net8.0/unit-net8.0.trx` | `artifacts/coverage/unit/net8.0.cobertura.xml` |
| `net9.0` | `artifacts/test-results/unit/net9.0/Serilog.Sinks.OpenSearch.Tests_net9.0_x64.trx` | `artifacts/coverage/unit/net9.0.cobertura.xml` |
| `net10.0` | `artifacts/test-results/unit/net10.0/Serilog.Sinks.OpenSearch.Tests_net10.0_x64.trx` | `artifacts/coverage/unit/net10.0.cobertura.xml` |

TUnit also emitted its HTML and JSON reports. Generated evidence under `/artifacts/` is ignored by the repository and is not intended for source control.

## Three-Run Repeatability

Three consecutive complete multi-target Release runs used `--report-trx --results-directory artifacts/test-results/unit/repeat-N`. All commands exited 0.

| Run | `net8.0` | `net9.0` | `net10.0` | Combined | TRX evidence |
|---|---|---|---|---|---|
| 1 | 59 passed, 5 skipped | 59 passed, 5 skipped | 59 passed, 5 skipped | 177 passed, 15 skipped, 0 failed | `artifacts/test-results/unit/repeat-1/*.trx` |
| 2 | 59 passed, 5 skipped | 59 passed, 5 skipped | 59 passed, 5 skipped | 177 passed, 15 skipped, 0 failed | `artifacts/test-results/unit/repeat-2/*.trx` |
| 3 | 59 passed, 5 skipped | 59 passed, 5 skipped | 59 passed, 5 skipped | 177 passed, 15 skipped, 0 failed | `artifacts/test-results/unit/repeat-3/*.trx` |

Discovery and outcomes were identical across all runs.

## CI and Documentation Validation

- PyYAML 6.0.3 parsed `.github/workflows/cicd.yaml` successfully. Its YAML 1.1 loader represents the standard GitHub Actions `on` key as boolean `True`; this is loader semantics, not a syntax error.
- CI invokes only `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` for tests, once per target framework.
- Producers write TRX below `artifacts/test-results/unit/<target>` and coverage to `artifacts/coverage/unit/<target>.cobertura.xml`.
- `actions/upload-artifact` consumes both producer paths, while `dorny/test-reporter` consumes `artifacts/test-results/unit/**/*.trx`.
- Legacy HTML upload and VSTest/GitHubActions logger arguments were removed.
- `README.md`, `quickstart.md`, and the execution contract use the same verified commands and state that integration migration is deferred.
- A live GitHub Actions run was unavailable in this local implementation session. Therefore SC-006 is configured and statically reconciled, but live publication remains unverified.

## Legacy-Reference Audit

An active-code/project audit found zero unit-owned xUnit imports, active xUnit attributes, `Xunit.Assert` calls, xUnit/VSTest packages, coverlet MSBuild packages, ReportGenerator packages, GitHubActionsTestLogger packages, or legacy logger commands. The remaining xUnit-looking lines in the unit directory are commented-out tests in `OpenSearchSinkTests.cs` and `OpenSearchLogShipperTests.cs`; they were deliberately left commented and are not active cases.

A repository-wide deferred-scope audit found 20 xUnit-related matching lines beneath `test/Serilog.Sinks.OpenSearch.IntegrationTests`. They belong to the excluded project and remain allowed exceptions.

## Production and Integration Boundaries

- `git diff -- src` returned no feature-driven production-source changes.
- The final solution SHA-256 still matches the captured baseline: `BC9F5960614E849EAB1432CECAE938AC99A3D24BFDFAF08E4CE858023BF91E66`. Integration solution membership is unchanged.
- The workflow contains no integration-test project command; this feature did not introduce integration execution.
- The feature made no edit under `test/Serilog.Sinks.OpenSearch.IntegrationTests/**`.

Three integration-tree differences appeared concurrently after the feature baseline was captured and were preserved as user-owned work:

| Difference | Current evidence |
|---|---|
| Deleted `Bootstrap/ClientTestClusterBase.cs` | Baseline hash `445AC2CF3DCB2840B3DCFA732CCE226F1206973C7C04D69CFC90BDE14ACC9EF6`; file now absent |
| Added `Bootstrap/OpenSearchTestBase.cs` | Current hash `5EC1398BFD77896E9E8A64F77F9D6AFDE5375685EBEB4F8D44AF04E7C33C84DD` |
| Changed integration `.csproj` | Baseline hash `33BB0B3373F5814B8E6FB666A0DD9C17573E05378153730ED130AB32B8786D0A`; current hash `F0023BD9D620D36E24F0C611A160CAE989EC0CF6E76CBA551439369ACBE41FD7` |

Every other captured integration hash matched. These changes are not unit-migration baseline differences and were neither interpreted nor modified.

The migration-scoped `git diff --check` (excluding the deferred integration directory) exited 0. The repository-wide check reports only two user-owned integration whitespace findings: `OpenSearchSinkOptionsFactory.cs:20` and the integration `.csproj:21`. They were not altered.

## Requirements Reconciliation

| Requirement | Evidence/status |
|---|---|
| FR-001–FR-005 | All 50 active methods and 64 expanded cases per target migrated; inputs, assertions, five skips, and reasons preserved |
| FR-006–FR-007 | TUnit cleanup hooks build and pass repeatedly; keyed non-parallel protection retained |
| FR-008–FR-009 | Integration feature scope untouched; 20 deferred xUnit matches classified above |
| FR-010–FR-011 | `net8.0;net9.0;net10.0` retained; complete, target, filter, debug, TRX, and coverage workflows documented |
| FR-012 | CI has per-target unit commands and matching TRX/Cobertura consumers; live CI publication unrun |
| FR-013 | Zero active legacy unit-owned references |
| FR-014 | No `src/**` diff |
| FR-015 | No unit inventory/outcome difference requires approval |
| FR-016 | README and feature quickstart document the TUnit workflow and deferred integration scope |
| FR-017 | Cleanup hooks completed without failure across all validated complete runs |
| SC-001–SC-005 | Met by discovery parity, skips, outcomes, audit, restore, and documented commands |
| SC-006 | **Incomplete live validation**: configured and statically reconciled; GitHub-hosted artifact publication was not run |
| SC-007–SC-009 | Met by three identical complete runs, empty production diff, and preserved deferred integration boundary |

There are no approved or unapproved differences in unit discovery or outcomes. The only validation gap is the explicitly unrun live GitHub Actions publication check.

## Assertion Library Follow-up — 2026-07-31

Following the maintainer's post-plan decision, FluentAssertions was removed from the unit project and all live checks were migrated to TUnit Assertions.

- The unit project has no FluentAssertions package in its restored asset graph.
- Scoped source and project searches find zero `FluentAssertions` imports, `.Should()` calls, or package references.
- There are 138 awaited `Assert.That(...)` statements and zero active unawaited TUnit assertions.
- Shared assertion helpers now return `Task`/`Task<T>` and their callers await them.
- Constructor-time request checks were moved to `[Before(Test)]` async hooks so TUnit assertions are awaited.
- Collection equivalency explicitly compares the returned file collection with a one-item expected collection; case-insensitive string equivalency uses `IsEqualTo(...).IgnoringCase()`.
- Release build passed for `net8.0`, `net9.0`, and `net10.0` with only the pre-existing `CS0672` warning.
- A focused `FileSetTests` run passed 18/18 across all targets after validating the collection-equivalency mapping.
- The complete suite passed with 177 succeeded, 15 intentionally skipped, and zero failed across 192 cases. Per target, the result remains 59 succeeded, five skipped, and zero failed.
- The integration-test project and production sources were not changed by this follow-up.
