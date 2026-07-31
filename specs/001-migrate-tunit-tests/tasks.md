# Tasks: Migrate Unit Tests to TUnit

**Input**: Design documents from `specs/001-migrate-tunit-tests/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/test-execution.md`, `quickstart.md`

**Tests**: This feature migrates the test suite itself. Tasks therefore include discovery, execution, lifecycle, reporting, and parity validation rather than adding unrelated product tests.

**Organization**: Tasks are grouped by user story so the unit migration, supported workflow, and deferred integration boundary can be implemented and verified independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it uses different files and has no dependency on another incomplete parallel task
- **[Story]**: Maps the task to User Story 1, 2, or 3
- Every task identifies the exact file or directory it changes or validates

## Phase 1: Setup (Shared Evidence)

**Purpose**: Capture the pre-implementation evidence needed to prove unit parity and preserve the deferred integration state.

- [X] T001 Create the pre-migration unit inventory in `specs/001-migrate-tunit-tests/migration-inventory.md`, recording the 35 C# files, 50 active annotated methods, 16 inline rows, one member-data source, five skip reasons, target frameworks, discoverable case counts, and current outcomes per framework
- [X] T002 [P] Capture the current user-owned state of `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` and `serilog-sinks-opensearch.sln` in `specs/001-migrate-tunit-tests/integration-boundary-baseline.md`, including file hashes or an equivalent reproducible diff baseline and the known pre-existing changes
- [X] T003 Extend `specs/001-migrate-tunit-tests/migration-inventory.md` with every active xUnit package, import, attribute, assertion, isolation declaration, coverage target, runner command, and CI logger owned by `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj`, `test/Serilog.Sinks.OpenSearch.Tests/**/*.cs`, and `.github/workflows/cicd.yaml`

**Checkpoint**: Unit parity and deferred-boundary baselines are recorded before any migration edit.

---

## Phase 2: Foundational (Blocking Runner Configuration)

**Purpose**: Establish the TUnit/Microsoft Testing Platform foundation required by every migrated unit test.

**⚠️ CRITICAL**: Complete this phase before converting unit-test source files.

- [X] T004 [P] Replace unit-project xUnit, VSTest, coverlet MSBuild, ReportGenerator, and GitHubActionsTestLogger references and coverage targets with the TUnit 1.63.0 meta-package requirements in `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj`, preserving `net8.0;net9.0;net10.0`, signing, resources, and production project references
- [X] T005 [P] Add the repository .NET 10 SDK selection and `Microsoft.Testing.Platform` test runner configuration to `global.json` without changing any project target framework

**Checkpoint**: The unit project has one intended TUnit runner/package path; source conversion may now begin.

---

## Phase 3: User Story 1 - Run the Complete Migrated Unit Suite (Priority: P1) 🎯 MVP

**Goal**: Every active unit test, data case, skip, assertion, lifecycle, and isolation rule is represented by an equivalent discoverable TUnit case on every declared target framework.

**Independent Test**: Discover and execute `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` for `net8.0`, `net9.0`, and `net10.0`, then reconcile logical tests, expanded data cases, five skips and their reasons, and outcomes with `specs/001-migrate-tunit-tests/migration-inventory.md`.

### Implementation for User Story 1

- [X] T006 [US1] Convert the five intentionally skipped facts to discoverable TUnit tests with their existing `Flaky test on GitHub actions` reason preserved in `test/Serilog.Sinks.OpenSearch.Tests/BulkActionTests.cs`
- [X] T007 [P] [US1] Convert facts and xUnit imports to TUnit without changing FluentAssertions expectations in `test/Serilog.Sinks.OpenSearch.Tests/CustomIndexTypeNameTests.cs`, `ExceptionAsJsonObjectFormatterTests.cs`, `IndexDeciderTests.cs`, `InlineFieldsTests.cs`, `PropertyNameTests.cs`, `RealExceptionTests.cs`, and `RealExceptionNoSerializerTests.cs`
- [X] T008 [P] [US1] Convert the six-row rolling-interval theory and replace `IDisposable` cleanup with equivalent TUnit per-test cleanup in `test/Serilog.Sinks.OpenSearch.Tests/FileSetTests.cs`
- [X] T009 [P] [US1] Convert both rolling-interval theories, preserve all six data rows, and replace `IDisposable` cleanup with equivalent TUnit per-test cleanup in `test/Serilog.Sinks.OpenSearch.Tests/OpenSearchPayloadReaderTests.cs`
- [X] T010 [P] [US1] Convert facts, the four inline data rows, and the member-data exception cases to typed TUnit tests and replace all live `Xunit.Assert` calls with equivalent FluentAssertions in `test/Serilog.Sinks.OpenSearch.Tests/OpenSearchJsonFormatterTests.cs`
- [X] T011 [P] [US1] Convert the serializer-discrepancy facts and xUnit imports to TUnit in `test/Serilog.Sinks.OpenSearch.Tests/Discrepancies/JsonNetSerializerTests.cs`, `NoSerializerTests.cs`, and `OpenSearchDefaultSerializerTests.cs` without changing `OpenSearchSinkUniformityTestsBase.cs`
- [X] T012 [P] [US1] Convert the standard template setup and registration facts to TUnit in `test/Serilog.Sinks.OpenSearch.Tests/Templating/DiscoverVersionTests.cs`, `DoNotRegisterTemplateIfItExists.cs`, `OverwriteTemplateTests.cs`, `RegisterCustomTemplateTests.cs`, and `SetOpenSearchSinkOptions.cs`
- [X] T013 [P] [US1] Convert the template-content facts to TUnit in `test/Serilog.Sinks.OpenSearch.Tests/Templating/SendsTemplateTests.cs`, `Sendsv1TemplateTests.cs`, `SetFiveReplicasInTemplateTests.cs`, `SetTwoShardsInTemplateTests.cs`, `SetZeroReplicasInTemplateTests.cs`, and `TemplateMatchTests.cs`, preserving all existing embedded-template assertions
- [X] T014 [P] [US1] Convert the unavailable-server facts and replace xUnit collection isolation with one equivalent keyed TUnit non-parallel rule in `test/Serilog.Sinks.OpenSearch.Tests/Templating/DiscoverVersionHandlesUnavailableServerTests.cs` and `SendsTemplateHandlesUnavailableServerTests.cs`
- [X] T015 [US1] Remove remaining active xUnit imports from comment-only or support sources in `test/Serilog.Sinks.OpenSearch.Tests/OpenSearchSinkTests.cs`, `OpenSearchLogShipperTests.cs`, and all other files under `test/Serilog.Sinks.OpenSearch.Tests/**` without re-enabling, deleting, or rewriting commented-out tests
- [X] T016 [US1] Restore and build `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` separately for `net8.0`, `net9.0`, and `net10.0`, correcting only migration-owned compile or analyzer failures in `test/Serilog.Sinks.OpenSearch.Tests/**`
- [X] T017 [US1] Run TUnit discovery for every declared target and record logical, expanded data-case, and skipped counts plus skip reasons against the baseline in `specs/001-migrate-tunit-tests/validation-evidence.md`
- [X] T018 [US1] Execute the complete unit suite on every declared target, verify pass/fail/skip parity and fixture cleanup behavior, and record outcomes or explicitly approved differences in `specs/001-migrate-tunit-tests/validation-evidence.md`

**Checkpoint**: User Story 1 is complete when the migrated unit suite independently builds, discovers, and runs with approved parity across all three targets.

---

## Phase 4: User Story 2 - Use One Supported Unit-Test Workflow (Priority: P2)

**Goal**: Contributors and CI can restore, build, run, filter, report, and collect coverage for the migrated unit project using one TUnit/Microsoft Testing Platform workflow.

**Independent Test**: From the documented prerequisites, execute the complete and filtered unit commands and confirm attributable TRX and Cobertura artifacts are produced without any unit-owned xUnit/VSTest tooling.

### Implementation for User Story 2

- [X] T019 [US2] Verify the installed TUnit/Microsoft Testing Platform list, tree-node filter, TRX, results-directory, and Cobertura command spellings, then update executable examples in `specs/001-migrate-tunit-tests/contracts/test-execution.md` and `specs/001-migrate-tunit-tests/quickstart.md`
- [X] T020 [P] [US2] Replace the legacy unit `dotnet test --logger` invocation, HTML artifact upload, and GitHubActions logger assumptions with an explicit unit-project TUnit command and attributable TRX/Cobertura artifacts in `.github/workflows/cicd.yaml`, without adding the integration project to CI
- [X] T021 [P] [US2] Document complete, per-target where supported, filtered, debug, result, and coverage unit workflows plus the deferred integration scope in `README.md`, using the verified commands from `specs/001-migrate-tunit-tests/quickstart.md`
- [X] T022 [US2] Execute a stable class or method tree-node filter against `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` and record the selected and excluded cases in `specs/001-migrate-tunit-tests/validation-evidence.md`
- [X] T023 [US2] Perform the documented restore, Release build, complete unit run, TRX generation, and Cobertura generation from the repository root and record artifact paths and command exit codes in `specs/001-migrate-tunit-tests/validation-evidence.md`
- [X] T024 [US2] Validate `.github/workflows/cicd.yaml` syntax and statically reconcile every test-result and coverage producer/consumer path, recording any unavailable live GitHub Actions validation separately in `specs/001-migrate-tunit-tests/validation-evidence.md`

**Checkpoint**: User Story 2 is complete when local commands and CI configuration expose one documented unit workflow with usable result and coverage evidence.

---

## Phase 5: User Story 3 - Defer Integration Migration Safely (Priority: P3)

**Goal**: Complete the unit migration without changing or partially processing the integration-test project.

**Independent Test**: Compare the final integration directory and solution state with `specs/001-migrate-tunit-tests/integration-boundary-baseline.md`, and confirm repository-wide xUnit matches are either removed from the unit scope or owned exclusively by the deferred integration scope.

### Implementation for User Story 3

- [X] T025 [P] [US3] Compare file hashes and diffs for `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` with `specs/001-migrate-tunit-tests/integration-boundary-baseline.md` and record that all pre-existing user changes were preserved with no feature-driven edit
- [X] T026 [P] [US3] Run a scoped legacy-reference audit across `test/Serilog.Sinks.OpenSearch.Tests/**`, `global.json`, `.github/workflows/cicd.yaml`, and `README.md`, then classify remaining repository-wide xUnit matches under `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` as deferred exceptions in `specs/001-migrate-tunit-tests/validation-evidence.md`
- [X] T027 [US3] Compare `serilog-sinks-opensearch.sln` and `.github/workflows/cicd.yaml` with the boundary baseline to prove the feature neither changes integration solution membership nor introduces integration execution, and record the result in `specs/001-migrate-tunit-tests/validation-evidence.md`

**Checkpoint**: User Story 3 is complete when the integration project, its user-owned edits, and its execution status match the captured boundary baseline.

---

## Phase 6: Polish & Cross-Cutting Validation

**Purpose**: Close the feature with repeatable evidence and explicit scope checks.

- [X] T028 Run the complete migrated unit suite three consecutive times on a stable environment and append per-target discovery, outcome, skip, and artifact evidence for all three runs to `specs/001-migrate-tunit-tests/validation-evidence.md`
- [X] T029 Verify there is no feature-driven diff under `src/**`, re-check the integration boundary against its captured baseline, and run a baseline-aware whitespace validation without altering the known pre-existing integration-project whitespace in `specs/001-migrate-tunit-tests/validation-evidence.md`
- [X] T030 Reconcile FR-001 through FR-017 and SC-001 through SC-009 against the completed evidence, leave any unmet or unrun validation explicitly incomplete, and record required approvals for every baseline difference in `specs/001-migrate-tunit-tests/validation-evidence.md`

## Phase 7: Assertion Library Follow-up

**Purpose**: Apply the maintainer's post-plan decision to use the TUnit assertion stack exclusively.

- [X] T031 Remove FluentAssertions from `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj`, migrate all live unit checks and assertion helpers to awaited TUnit Assertions, preserve comparison semantics and setup validation, and rerun the complete three-target suite

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 — Setup**: No dependencies; T001 and T002 may begin together, while T003 follows T001
- **Phase 2 — Foundational**: Depends on Phase 1; T004 and T005 can run in parallel and jointly block source conversion
- **Phase 3 — User Story 1**: Depends on Phase 2; conversion tasks T006–T015 precede build/discovery/execution tasks T016–T018
- **Phase 4 — User Story 2**: Depends on a runnable User Story 1; T020 and T021 can run in parallel after command verification in T019
- **Phase 5 — User Story 3**: Uses the Phase 1 boundary baseline and must run after all desired implementation edits so its final comparison is meaningful
- **Phase 6 — Polish**: Depends on the desired user stories; T030 follows all validation evidence

### User Story Dependencies

- **User Story 1 (P1)**: Starts after the runner foundation and has no dependency on another user story
- **User Story 2 (P2)**: Depends on User Story 1's runnable migrated suite but is independently testable through its documented command/artifact contract
- **User Story 3 (P3)**: Has no behavioral dependency on the migrated suite; its final boundary comparison occurs after other edits to prove isolation

### Dependency Graph

```text
Setup (T001-T003)
├── Foundation (T004-T005) -> US1 (T006-T018) -> US2 (T019-T024) ─┐
└── Integration boundary baseline ───────────────> US3 (T025-T027) ├-> Polish (T028-T030)
                                                                  ┘
```

### Within Each User Story

- Perform source conversions before compiling and discovering tests
- Build every target before comparing discovery or outcomes
- Verify runner command syntax before updating CI and contributor documentation
- Capture the integration baseline before implementation and compare it only after feature edits are complete
- Record evidence honestly; an unavailable or unrun check remains incomplete rather than being inferred from another success

## Parallel Opportunities

- T001 and T002 capture different baselines and can run concurrently
- T004 and T005 edit different runner-configuration files
- T007 through T014 operate on disjoint unit-test source groups after T004 completes
- T020 and T021 update CI and contributor documentation independently after T019
- T025 and T026 perform independent boundary and scoped-reference audits

## Parallel Example: User Story 1

```text
Task T007: Convert core fact-only test files under test/Serilog.Sinks.OpenSearch.Tests/
Task T008: Convert FileSetTests.cs data and cleanup lifecycle
Task T009: Convert OpenSearchPayloadReaderTests.cs data and cleanup lifecycle
Task T010: Convert OpenSearchJsonFormatterTests.cs data sources and assertions
Task T011: Convert Discrepancies/*.cs tests
Task T012: Convert standard Templating/*.cs setup tests
Task T013: Convert Templating/*.cs content tests
Task T014: Convert isolated unavailable-server template tests
```

## Parallel Example: User Story 2

```text
After T019 verifies the final commands:
Task T020: Update .github/workflows/cicd.yaml
Task T021: Update README.md
```

## Parallel Example: User Story 3

```text
Task T025: Compare the deferred integration tree with its captured baseline
Task T026: Audit unit-owned and deferred xUnit references
```

## Implementation Strategy

### MVP First — User Story 1

1. Complete Setup tasks T001–T003.
2. Complete Foundation tasks T004–T005.
3. Complete User Story 1 tasks T006–T018.
4. Stop and independently validate unit discovery and outcomes on all declared targets.

This MVP proves behavioral parity for the migrated unit suite. It does not complete the supported CI/documentation workflow or the final deferred-boundary proof.

### Incremental Delivery

1. **Baseline + Foundation**: Establish evidence and the TUnit runner path.
2. **User Story 1**: Deliver a complete runnable unit migration.
3. **User Story 2**: Deliver the contributor and CI workflow with artifacts.
4. **User Story 3**: Prove the deferred integration scope was preserved.
5. **Polish**: Establish three-run repeatability and close requirements with honest evidence.

## Notes

- `[P]` tasks modify different files or perform independent read-only checks.
- Integration-project files and production sources are validation targets only, never implementation targets.
- Existing integration-project and solution changes belong to the user and must remain intact.
- Do not fix the known integration `.csproj` trailing whitespace as part of this feature.
- Every checkbox remains unchecked until that exact task and its stated validation have been completed.
