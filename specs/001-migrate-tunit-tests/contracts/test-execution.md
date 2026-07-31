# Contract: Unit-Test Execution and Evidence

## Purpose

This contract defines how contributors and CI discover, execute, filter, and collect evidence from the migrated TUnit unit suite. It does not define or change integration-test execution. Exact output directories may be parameterized, but each artifact must remain attributable to its unit target framework.

## Prerequisites

- A .NET 10 SDK honoring the repository `global.json` and runtimes required by the unit project's declared target frameworks.
- Restored NuGet dependencies.
- No external service or Docker prerequisite for the in-scope unit run.

## Supported Commands

| Capability | Contract |
|---|---|
| Restore | `dotnet restore test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` succeeds from a clean checkout. |
| Build | `dotnet build test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-restore` builds every declared unit target. |
| Unit suite | `dotnet test test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj -c Release --no-build` executes every declared unit target. |
| Per-target suite | Add `-f net8.0`, `-f net9.0`, or `-f net10.0` before the runner options. |
| Focused selection | Add `--treenode-filter "/*/*/OpenSearchJsonFormatterTests/*"`; this verified query selects the 13 formatter cases and excludes the other 51 cases for one target. |
| Results | Add `--report-trx --results-directory <target-specific-path>` to produce machine-readable TRX. `--report-trx-filename <name>` is available when an explicit filename is needed. |
| Coverage | Add `--coverage --coverage-output-format cobertura --coverage-output <target-specific-file>` to produce Cobertura output. |

These spellings were verified with TUnit 1.63.0 and the repository-selected .NET 10.0.302 SDK. Runner switches are passed directly to `dotnet test`; use `--` before runner switches with `dotnet run --project`.

## Discovery Contract

1. Each active pre-migration logical unit test has a migrated TUnit identity.
2. Every inline/member-data input expands to an independently attributable unit case.
3. Every intentional unit skip is discoverable and includes its reason.
4. Counts are compared per target framework; multi-target repetition is not counted as duplication.
5. Commented-out unit-test code is excluded from both baselines.
6. Any difference is recorded with reason and approval before completion.

## Result and Coverage Contract

- CI emits TRX evidence for the migrated unit project, with target-framework disambiguation where the runner emits separate runs.
- Existing CI test-report publication consumes all emitted unit TRX files.
- Coverage runs emit Cobertura XML in a predictable unit-specific location suitable for upload or report generation.
- Legacy HTML and GitHubActions logger outputs are not contractual; they may be removed when unit TRX reporting remains visible in CI.
- Integration-test results and coverage are outside this contract.
- Multi-target result and coverage commands use distinct per-target paths so concurrent target processes cannot overwrite one another.

## Exit and Failure Contract

| Condition | Required behavior |
|---|---|
| Assertion or product failure | Non-zero unit-test command; failure attributed to the relevant unit case. |
| Test/lifecycle code failure | Non-zero unit-test command; stage and owning fixture/test visible. |
| Cancellation | Unit run stops and attempts disposal of every created unit-test resource. |
| Cleanup failure | Visible non-successful lifecycle outcome; never silently swallowed. |
| No filter matches | Clear zero-match result and no implication that the complete unit suite passed. |

## Isolation and Lifecycle Contract

- Unit tests remain parallel unless their actual shared state requires a named isolation key.
- Fixture readiness completes before dependent unit-test execution.
- Cleanup is awaited, safe to retry where appropriate, and attempted after pass, failure, skip, or cancellation.
- No integration fixture, cluster, bootstrap, or external-service lifecycle is introduced or modified by this contract.

## Deferred Integration Boundary

- `test/Serilog.Sinks.OpenSearch.IntegrationTests` remains unchanged.
- Its current xUnit references are allowed scoped exceptions to the unit migration audit.
- It is not added to or removed from solution/CI execution by this feature.
- No command in this contract claims to execute the integration project.
- A separate specification must define its eventual discovery, lifecycle, environment, reporting, and cleanup contract.

## Completion Evidence

The unit migration is complete only when:

1. Unit discovery and outcomes reconcile with the approved baseline.
2. CI executes the migrated unit project and publishes its agreed artifacts.
3. Three consecutive stable complete unit runs have consistent discovery and outcomes.
4. Scoped inspection finds no active xUnit package, import, attribute, runner, configuration, suppression, or command in the unit project or unit-only workflow.
5. Production-library source and the deferred integration-test project have no feature-driven change.
