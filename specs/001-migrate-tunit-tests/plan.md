# Implementation Plan: Migrate Unit Tests to TUnit

**Branch**: `dev` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-migrate-tunit-tests/spec.md`

## Summary

Replace xUnit and its runners with TUnit 1.63.0 in `test/Serilog.Sinks.OpenSearch.Tests` while preserving every active unit case, data row, skip reason, target framework, fixture lifetime, isolation rule, and assertion intent. Use TUnit Assertions throughout and standardize the unit workflow on Microsoft Testing Platform with TRX and Cobertura output. Explicitly exclude `test/Serilog.Sinks.OpenSearch.IntegrationTests`: its project, dependencies, cluster infrastructure, test sources, and current solution/execution status remain unchanged until a separate feature handles them.

## Technical Context

**Language/Version**: C# on .NET 8.0, .NET 9.0, and .NET 10.0; .NET 10 SDK orchestrates unit builds and tests

**Primary Dependencies**: TUnit 1.63.0 (including TUnit Assertions), Serilog, OpenSearch.Client v2

**Storage**: N/A

**Testing**: TUnit on Microsoft Testing Platform, `dotnet test`, TUnit tree-node filters, TRX test results, and Cobertura coverage

**Target Platform**: Windows and Linux developer/CI hosts supported by the unit project's declared frameworks

**Project Type**: Multi-targeted .NET library with an in-scope unit-test project; the separate external-service integration project is deferred

**Performance Goals**: Preserve practical unit-test feedback time and safe parallel execution while serializing only unit tests with proven shared-state constraints

**Constraints**: Zero production-library behavior changes; preserve all unit target frameworks; preserve unit-test cleanup on all outcomes; no active xUnit tooling remains in the unit project or unit-only execution path; no changes under the integration-test project

**Scale/Scope**: One unit-test project, 35 C# files, 50 active annotated test methods, 16 inline data rows, one member-data source, and five intentional skips

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1.*

The repository constitution is an unratified placeholder and therefore defines no enforceable project gates. The feature specification supplies the operative constraints.

| Gate | Pre-research | Post-design | Evidence |
|---|---|---|---|
| Production behavior remains unchanged | PASS | PASS | Changes are restricted to the unit test project, unit workflow configuration, CI, and contributor documentation. |
| Complete in-scope inventory is preserved | PASS | PASS | Research defines an inventory-first unit conversion and contracts require discovery comparison across all unit targets and data rows. |
| Integration work is excluded | PASS | PASS | The integration project and its project-specific infrastructure are explicit protected boundaries in the spec, plan, contract, and validation guide. |
| No unresolved technical clarification remains | PASS | PASS | Unit framework, assertion, runner, reporting, lifecycle, and audit choices are resolved in `research.md`. |

## Project Structure

### Documentation (this feature)

```text
specs/001-migrate-tunit-tests/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── test-execution.md
└── tasks.md                 # Created later by /speckit-tasks
```

### Source Code (repository root)

```text
src/
├── Serilog.Sinks.OpenSearch/
└── Serilog.Sinks.OpenSearch.Sinks.DurableHttpUsingTimeRolledBuffers/

test/
├── Serilog.Sinks.OpenSearch.Tests/                 # In scope
│   ├── Serilog.Sinks.Opensearch.Tests.csproj
│   └── **/*.cs
└── Serilog.Sinks.OpenSearch.IntegrationTests/      # Explicitly excluded

.github/workflows/cicd.yaml                         # Unit workflow changes only
Serilog.Sinks.OpenSearch.sln                        # Preserve integration exclusion
global.json                                         # Planned Microsoft Testing Platform selection
```

**Structure Decision**: Migrate the existing multi-targeted unit project in place. Do not add, remove, edit, retarget, or otherwise process the integration-test project or its project-specific files. Keep its current solution status unchanged. Shared workflow edits must affect the unit path only and must not imply that repository-wide integration migration is complete.

## Implementation Strategy

### A. Establish the unit-migration baseline

- Record logical unit-test methods, expanded parameter rows, intentional skips and reasons, target frameworks, and expected outcomes before edits.
- Record unit-project discovery and results per target framework where the current environment permits.
- Capture xUnit packages, imports, attributes, runners, and CI commands owned by the unit project or its unit-only execution path.
- Record current integration-project file hashes or an equivalent scoped diff baseline solely to prove that the excluded project remains unchanged; do not inspect or redesign its behavior as part of implementation.

### B. Convert the unit project

- Replace xUnit packages, runners, and FluentAssertions in the unit project with the TUnit meta-package and awaited TUnit Assertions.
- Map facts, theories, inline/member data, skips, fixtures, cleanup, and isolation to typed TUnit equivalents.
- Convert all live xUnit and FluentAssertions checks to awaited `Assert.That(...)` assertions without changing expected behavior.
- Validate each unit target framework independently and compare discovery to the baseline before broad cleanup.

### C. Standardize the unit execution workflow

- Select Microsoft Testing Platform through the .NET 10 `global.json` test-runner setting.
- Update CI to run the migrated unit project and publish TUnit-compatible TRX and Cobertura artifacts.
- Remove legacy HTML/GitHubActions/xUnit runner paths only where they belong to unit execution.
- Document complete, per-target or per-project, filtered, debug, and coverage workflows, with an explicit notice that integration testing remains deferred.
- Do not add the integration project to the solution or CI and do not remove its xUnit dependencies.

### D. Close the unit migration

- Compare post-migration unit discovery and outcomes against the recorded baseline, including all parameter rows and skips.
- Run three complete stable-environment unit validations and confirm consistent counts and outcomes.
- Audit the unit project and unit-only workflows for zero active xUnit references while treating references owned exclusively by the deferred integration project as documented exceptions.
- Verify that the implementation introduces no production-source change and that the integration project still matches the scoped pre-implementation baseline, preserving any changes that already existed when implementation began.
- Record any intentional unit-baseline difference and require explicit approval before completion.

## Deferred Work Boundary

The integration-test project requires a later, separate specification and plan. This feature makes no technology, lifecycle, dependency, target-framework, solution, or CI decision for that future work.
