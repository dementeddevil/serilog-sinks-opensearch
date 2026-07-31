# Validation Evidence

## Baseline - 2026-07-31

- Branch: `dev`
- .NET SDK: `10.0.302`
- No solution command was invoked.
- Scoped restores: formatter, sink, unit-test, and sample projects restored successfully. The unit-test restore reported `NU1900` because the NuGet vulnerability feed was unavailable; dependency restore still succeeded.
- Release builds: formatter (0 warnings/errors), sink (0 warnings/errors), unit tests (6 pre-existing warnings, 0 errors), sample (1 pre-existing obsolete-API warning, 0 errors).
- Direct TUnit executable results:
  - `net8.0`: 80 discovered, 75 passed, 5 skipped, 0 failed.
  - `net9.0`: 80 discovered, 75 passed, 5 skipped, 0 failed.
  - `net10.0`: 80 discovered, 75 passed, 5 skipped, 0 failed.
- The five skips are existing GitHub Actions flaky-test exclusions. The unit warnings are `NU1900` and the existing obsolete serialization override.

## Feature validation

- Test-first formatter run on `net8.0`: 7 discovered, 1 passed, 6 failed before production changes; failures demonstrated absent trace/span output.
- User Story 1/2 focused run on `net8.0`: 18 discovered, 18 passed, 0 skipped/failed, including fragments, exception formatter, collisions, rendering, and 10,000-event concurrency.
- Complete functional `TraceContext*Tests` run after sink configuration/ownership work: 27 functional cases passed; the separately filterable benchmark was the only failure.
- Initial benchmark: runtime 8.0.29, baseline samples 36.868/38.104/39.280/44.730/68.120/92.376/131.267 ms, candidate 39.801/40.722/43.758/48.643/49.445/103.667/148.408 ms, median ratio 1.0875. This exceeded 1.05 and had unstable samples; writer dispatch was optimized and the benchmark was given an explicit idle/stability guard. SC-008 remains unverified pending a guarded rerun.

## Final scoped validation - 2026-07-31

- Quickstart restores and Release builds completed for formatter, sink, unit-test, and sample project files only; no solution or integration command was invoked. Formatter and sink builds had zero warnings/errors. Unit builds retained the pre-existing obsolete-serialization warning; sample retained its pre-existing obsolete `FileSink` warning.
- Non-benchmark `TraceContext*Tests`: 28 passed, 0 failed/skipped on each of `net8.0`, `net9.0`, and `net10.0`.
- Complete suites: each target discovered 109, passed 104, retained the same 5 skips, and failed 0. This reconciles the 80-test baseline plus 29 new tests (28 functional and 1 separately filterable benchmark).
- Guarded benchmark rerun on .NET 8.0.29: baseline samples 17.271/18.011/18.185/45.502/85.054/101.643/105.187 ms; candidate 18.053/19.789/20.634/23.507/66.566/104.293/108.690 ms; reported ratio 0.5166; idle guard **failed** because sample spread exceeded 50%. The ratio is not accepted as performance proof, so SC-008 remains explicitly unverified.
- Packages: `Serilog.Formatting.OpenSearch` 3.0.0 and `Serilog.Sinks.OpenSearch` 2.0.0 packed successfully. Nuspec inspection confirms the sink depends on formatter 3.0.0 and preserves all other dependency metadata. Existing positional constructors and existing/new named sink configuration calls compile in the compatibility tests.
- Audits: production contains no `Activity.Current` reference; all trace references are expected formatter/options/factory/docs/sample references; custom and compact ownership has exact-output/event-reference tests; `git diff --check` passed after removing one trailing space.
- Protected boundary: solution and integration source SHA-256 values exactly match `integration-boundary-baseline.md`; scoped status remains empty. Zero feature-driven edits are proven.

## Requirement reconciliation

- FR-001 through FR-020: implemented and covered by focused tests, source audit, documentation, and package validation.
- SC-001 through SC-007: satisfied by the 28-case cross-target functional suite and complete regression runs.
- SC-008: **incomplete/unverified** because the controlled benchmark's idle/stability guard failed. No exception was approved and no passing claim is made.

## User-authorized live integration follow-up - 2026-07-31

- Scope expansion: the existing integration project was explicitly brought into scope after the original protected-boundary implementation; the solution remains unchanged.
- Integration restore succeeded and the Release build completed with 0 warnings and 0 errors.
- Added one reusable indexed-document scenario and registered it against both existing fixtures: OpenSearch 1.3.20 and OpenSearch 2.19.6.
- The scenario verifies default canonical event-owned root identifiers, untraced omission, nested case-insensitive collision suppression and property fallback, ordinary-property retention, inline fields, and independent `RenderTraceId` opt-out with `SpanId` still authoritative.
- An initial execution used the pre-correction binary and failed only at two obsolete length assertions after the indexed identifier equality checks had passed. After rebuilding with `Length()`, the complete integration executable discovered 6 tests and passed 6, with 0 failed/skipped, in 30.145 seconds.
- Docker Desktop server 29.6.2 supplied the ephemeral Linux containers. TRX evidence: `test/Serilog.Sinks.OpenSearch.IntegrationTests/bin/Release/net10.0/TestResults/integration-full-rerun.trx`.
