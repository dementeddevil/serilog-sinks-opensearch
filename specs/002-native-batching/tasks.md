# Tasks: Adopt Native Serilog Batching

**Input**: Design documents from `specs/002-native-batching/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/native-batching.md`, `quickstart.md`

**Tests**: The specification explicitly requires deterministic unit and contract coverage for retry, failure listeners, fallback chains, queue modes, partial bulk responses, disposal, compatibility, and package dependencies. Test tasks therefore precede their corresponding implementation tasks.

**Organization**: Tasks are grouped by user story so temporary-failure recovery, standard failure routing, and compatibility/package operation can be implemented and validated as separate increments. The integration-test project is a protected exclusion throughout.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it uses different files and has no dependency on another incomplete parallel task
- **[Story]**: Maps the task to User Story 1, 2, or 3
- Every task identifies the exact file or directory it changes, creates, or validates

## Phase 1: Setup (Shared Evidence)

**Purpose**: Capture the pre-implementation product, unit, compatibility, and protected-boundary evidence required for honest regression validation.

- [X] T001 Create `specs/002-native-batching/validation-evidence.md` and record the current branch, SDK, scoped production/unit/sample restore and Release-build results, plus unit discovery and outcomes for `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` on `net8.0`, `net9.0`, and `net10.0` without invoking the solution
- [X] T002 [P] Capture the existing user-owned state of `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` and `serilog-sinks-opensearch.sln` in `specs/002-native-batching/integration-boundary-baseline.md` using reproducible file hashes or equivalent scoped diff evidence without restoring, building, testing, or modifying the integration project
- [X] T003 [P] Record the pre-change public constructor, `Emit`, `Dispose`, `ILogEventSink`, level restriction, level switch, direct dependency, and packed dependency expectations in `specs/002-native-batching/compatibility-baseline.md` using `src/Serilog.Sinks.OpenSearch/**`, `README.md`, and a baseline package inspection

**Checkpoint**: Product behavior, public source patterns, package dependencies, unit outcomes, and the deferred integration state are recorded before implementation edits.

---

## Phase 2: Foundational (Deterministic Test Infrastructure)

**Purpose**: Provide shared, non-networked test doubles and synchronization primitives required by every user story.

**⚠️ CRITICAL**: Complete this phase before writing story-specific native-batching tests.

- [X] T004 [P] Add a queued-response, exception, request-capture, and synchronization-gate `IConnection` double in `test/Serilog.Sinks.OpenSearch.Tests/Stubs/ScriptedConnection.cs` without any live OpenSearch dependency
- [X] T005 [P] Add a thread-safe `ILoggingFailureListener` recorder for failure kind, exception, source, message, and exact affected events in `test/Serilog.Sinks.OpenSearch.Tests/Stubs/RecordingFailureListener.cs`
- [X] T006 [P] Add a thread-safe `ILogEventSink` fallback collector with optional injected failure behavior in `test/Serilog.Sinks.OpenSearch.Tests/Stubs/CollectingSink.cs`

**Checkpoint**: Story tests can deterministically control requests, recovery, listener notifications, fallback delivery, and shutdown without fixed sleeps or a live cluster.

---

## Phase 3: User Story 1 - Recover from Temporary OpenSearch Delivery Failures (Priority: P1) 🎯 MVP

**Goal**: Every invalid whole-batch outcome propagates to native Serilog batching, which retries the same batch within `RetryTimeLimit`, recovers without duplicates, and drops/report losses when the retry window is exhausted.

**Independent Test**: Configure a bounded retry window with `ScriptedConnection`, fail the first whole-batch attempt, allow the next attempt to succeed, and verify the identical event set is retried and delivered once; separately configure zero retry and verify first-failure exhaustion.

### Tests for User Story 1

> **NOTE**: Write these tests first and confirm the relevant assertions fail against the compatibility implementation before changing production code.

- [ ] T007 [US1] Add failing TUnit cases for thrown transport/payload exceptions, unsuccessful responses with and without `OriginalException`, unconditional propagation regardless of `ThrowException`, and `OnEmptyBatchAsync()` in `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingDeliveryTests.cs`
- [ ] T008 [US1] Extend `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingDeliveryTests.cs` with synchronization-gated native retry-then-success, exact same-batch/no-duplicate delivery, success-state reset, zero-retry exhaustion, and generous deadlock-guard cases

### Implementation for User Story 1

- [X] T009 [US1] Add nullable `RetryTimeLimit`, accept queue sentinel `-1`, accept positive queue bounds, and reject queue zero or values below `-1` in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/OpenSearchSinkOptions.cs`
- [X] T010 [US1] Replace periodic inheritance with the native `IBatchedLogEventSink` transport plus an `ILogEventSink`/`IDisposable` public façade created through `LoggerSinkConfiguration.CreateSink()`, centralize `BatchingOptions` mapping, and propagate every invalid whole-batch outcome in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/OpenSearchSink.cs`
- [X] T011 [US1] Register non-durable mode through `Sink(IBatchedLogEventSink, BatchingOptions, minimumLevel, levelSwitch)` while preserving the ordinary durable registration branch in `src/Serilog.Sinks.OpenSearch/LoggerConfigurationOpenSearchExtensions.cs`
- [ ] T012 [US1] Run the `NativeBatchingDeliveryTests` filter for `net8.0`, verify retry/recovery and zero-exhaustion outcomes without fixed timing assertions, and append commands, counts, and results to `specs/002-native-batching/validation-evidence.md`

**Checkpoint**: User Story 1 is complete when temporary whole-batch failures are retried by native Serilog, recovery delivers the batch without duplicates, and exhausted failures are no longer silently reported as success.

---

## Phase 4: User Story 2 - Observe and Route Batching Failures Consistently (Priority: P2)

**Goal**: Native failure listeners and fallback chains receive temporary, permanent, and final batching outcomes, while legacy handlers remain limited to rejected items in valid completed bulk responses.

**Independent Test**: Attach a recording listener and fallback, induce attempt failure, retry exhaustion, valid partial rejection, successful disposal flush, and failed shutdown; verify standard failure kinds, exact affected events, fallback routing, and absence of duplicate legacy handling.

### Tests for User Story 2

> **NOTE**: Write these tests first and confirm listener/fallback/lifecycle assertions fail before completing the story implementation.

- [ ] T013 [P] [US2] Add failing TUnit cases for `Temporary`, retry-exhaustion `Permanent`, fallback-only-on-permanent/final, default SelfLog availability, valid partial bulk rejection, and throwing legacy handler containment in `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingFailureTests.cs`
- [ ] T014 [P] [US2] Add failing synchronization-gated TUnit cases for successful disposal flush, failed current shutdown batch, remaining-queue `Final` reporting, and bounded disposal completion in `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingLifecycleTests.cs`

### Implementation for User Story 2

- [X] T015 [US2] Implement `ISetLoggingFailureListener` on the public façade and delegate listener attachment to the native batching wrapper in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/OpenSearchSink.cs`
- [X] T016 [US2] Restrict `EmitEventFailure`, `FailureSink`, `FailureCallback`, and item SelfLog processing to rejected items in valid completed responses, preserve containment of handler exceptions, and remove whole-batch legacy handler invocation in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/OpenSearchSink.cs`
- [ ] T017 [US2] Run the `NativeBatchingFailureTests` and `NativeBatchingLifecycleTests` filters for `net8.0`, verify listener/fallback/item/disposal contracts independently, and append exact outcomes to `specs/002-native-batching/validation-evidence.md`

**Checkpoint**: User Story 2 is complete when native listeners and fallbacks own every whole-batch lifecycle failure and legacy handlers process only terminal rejected items without duplicating accepted or retryable events.

---

## Phase 5: User Story 3 - Configure and Operate Batching without a Compatibility Package (Priority: P3)

**Goal**: Consumers retain documented source usage and batching behavior, can configure retry from code or named configuration, and restore a package with no periodic-batching dependency while durable shipping remains unchanged.

**Independent Test**: Build direct construction and `WriteTo.OpenSearch` consumer patterns, verify every option mapping and validation rule, exercise batching shape/eager behavior and durable routing, then inspect the packed dependency metadata for zero compatibility-package references.

### Tests for User Story 3

> **NOTE**: Write these tests first and confirm configuration/compatibility assertions fail before final production and packaging changes.

- [X] T018 [P] [US3] Add failing TUnit cases for batch size, buffering period, eager-first-event, positive queue, `-1` unbounded queue, invalid queue values, null native retry default, zero/positive retry, and negative retry construction failure in `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingOptionsTests.cs`
- [ ] T019 [P] [US3] Add failing source-contract TUnit cases for public construction, `ILogEventSink`, `Emit`, `using`/`Dispose`, failure-listener forwarding, level restriction, level switch, non-durable registration, and unchanged durable routing in `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingCompatibilityTests.cs`
- [ ] T020 [P] [US3] Add failing synchronization-gated TUnit cases for configured batch partitioning, eager first emission before a deliberately long period, non-overlapping batch callbacks, and accepted-event uniqueness in `test/Serilog.Sinks.OpenSearch.Tests/NativeBatchingConfigurationTests.cs`

### Implementation for User Story 3

- [X] T021 [US3] Append nullable named `retryTimeLimit` configuration, map it to `OpenSearchSinkOptions.RetryTimeLimit`, and preserve all existing optional call sites in `src/Serilog.Sinks.OpenSearch/LoggerConfigurationOpenSearchExtensions.cs`
- [X] T022 [US3] Remove the `Serilog.Sinks.PeriodicBatching` package reference while preserving Serilog 4.4.0 and all other package metadata in `src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj`
- [X] T023 [US3] Document retry configuration/default/zero behavior, native listeners and fallback chains, item-level legacy handlers, disposal behavior, direct construction, and the Serilog 4.4 overflow limitation in `README.md`
- [X] T024 [P] [US3] Update retry and failure-handling usage without changing durable behavior in `sample/Serilog.Sinks.OpenSearch.Sample/Program.cs` and `sample/Serilog.Sinks.OpenSearch.Sample/appsettings.json`
- [X] T025 [P] [US3] Replace the stale periodic-batching XML reference without changing durable retry scheduling in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/Durable/ExponentialBackoffConnectionSchedule.cs`
- [X] T026 [US3] Run all `NativeBatching*Tests` for `net8.0`, build `sample/Serilog.Sinks.OpenSearch.Sample/Serilog.Sinks.OpenSearch.Sample.csproj` as a consumer, and append configuration/compatibility outcomes to `specs/002-native-batching/validation-evidence.md`
- [X] T027 [US3] Pack `src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj`, inspect the generated `.nuspec`, and record proof of zero direct or transitive `Serilog.Sinks.PeriodicBatching` dependency in `specs/002-native-batching/validation-evidence.md`

**Checkpoint**: User Story 3 is complete when existing source usage builds, native settings retain established behavior, retry is configurable, the sample compiles, durable routing is unchanged, and the packed dependency is absent.

---

## Phase 6: Polish & Cross-Cutting Validation

**Purpose**: Close the feature with changelog guidance, broad scoped builds/tests, repeatability, dependency/reference audits, and protected-boundary evidence.

- [X] T028 [P] Add the native-batching migration, retry/failure behavior change, compatibility boundary, and removed package dependency under `[Unreleased]` in `CHANGES.md`
- [X] T029 Execute the scoped restore and Release-build sequence from `specs/002-native-batching/quickstart.md` for production, unit, and sample projects only, and append command exit codes and outcomes to `specs/002-native-batching/validation-evidence.md`
- [X] T030 Run all `NativeBatching*Tests` on `net8.0`, `net9.0`, and `net10.0`, recording per-target discovery and outcomes in `specs/002-native-batching/validation-evidence.md`
- [ ] T031 Run the complete `test/Serilog.Sinks.OpenSearch.Tests/Serilog.Sinks.Opensearch.Tests.csproj` suite three consecutive times, reconcile the 64-case-per-target baseline plus enumerated new cases and five retained skips, and append all results to `specs/002-native-batching/validation-evidence.md`
- [X] T032 Run the scoped compatibility-reference, dependency, package-content, and whitespace audits from `specs/002-native-batching/quickstart.md`, classify only legitimate historical references, and record results in `specs/002-native-batching/validation-evidence.md`
- [ ] T033 Compare `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` and `serilog-sinks-opensearch.sln` with `specs/002-native-batching/integration-boundary-baseline.md`, preserving every pre-existing user change and recording zero feature-driven edits in `specs/002-native-batching/validation-evidence.md`
- [ ] T034 Reconcile FR-001 through FR-022 and SC-001 through SC-010 against actual implementation and evidence, leave unavailable or failed checks explicitly incomplete, and record any approved exception in `specs/002-native-batching/validation-evidence.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 — Setup**: No dependencies; T001, T002, and T003 may begin together
- **Phase 2 — Foundational**: Depends on Phase 1 so baseline counts and files are captured before adding tests; T004, T005, and T006 can run in parallel
- **Phase 3 — User Story 1**: Depends on Phase 2; tests T007–T008 precede production tasks T009–T011, and T012 validates the complete story
- **Phase 4 — User Story 2**: Depends on User Story 1's native batching path; tests T013–T014 precede T015–T016, and T017 validates the complete story
- **Phase 5 — User Story 3**: Depends on User Story 1's native registration and façade; tests T018–T020 precede T021–T025, followed by validation T026–T027
- **Phase 6 — Polish**: Depends on all desired user stories; final boundary and requirement reconciliation occur only after all feature edits and validations

### User Story Dependencies

- **User Story 1 (P1)**: Starts after shared test infrastructure and has no dependency on another story; this is the MVP
- **User Story 2 (P2)**: Uses User Story 1's native batching wrapper so listener/fallback and shutdown behavior can be observed; it remains independently testable through its own failure and lifecycle filters
- **User Story 3 (P3)**: Uses User Story 1's native option mapper/registration but has independent configuration, source-compatibility, durable-routing, sample, and package acceptance tests

### Dependency Graph

```text
Setup (T001-T003)
  → Test foundation (T004-T006)
    → US1 Retry and Recovery (T007-T012) ─┬→ US2 Failure Routing (T013-T017) ─┐
                                          └→ US3 Configuration/Package (T018-T027) ─┤
                                                                                     └→ Polish (T028-T034)
```

### Within Each User Story

- Add the story's tests and verify the relevant assertions fail before implementation
- Complete public option/contract changes before wiring dependent registration behavior
- Complete transport/facade behavior before running logger-level retry, listener, fallback, or disposal cases
- Run focused story tests before broad multi-target validation
- Record unavailable or failed validation honestly rather than inferring success from another check

## Parallel Opportunities

- T001, T002, and T003 capture independent baseline evidence
- T004, T005, and T006 create separate shared test doubles
- T013 and T014 cover failure routing and lifecycle in separate files
- T018, T019, and T020 cover options, compatibility, and batching shape in separate files
- T024 and T025 update the sample and a durable documentation comment independently after core configuration is complete
- T028 can update the changelog while final scoped builds/tests begin, provided it reflects only completed behavior

## Parallel Example: User Story 1

```text
Task T007: Add direct transport and unsuccessful-response cases in NativeBatchingDeliveryTests.cs
Task T008: After T007 establishes the file, add logger-level recovery/exhaustion cases in NativeBatchingDeliveryTests.cs
```

User Story 1 tests share one file and are intentionally sequential; implementation begins only after both failing contracts are present.

## Parallel Example: User Story 2

```text
Task T013: Add listener, fallback, and partial-item cases in NativeBatchingFailureTests.cs
Task T014: Add disposal and final-failure cases in NativeBatchingLifecycleTests.cs
```

## Parallel Example: User Story 3

```text
Task T018: Add option mapping/validation cases in NativeBatchingOptionsTests.cs
Task T019: Add public source and durable routing cases in NativeBatchingCompatibilityTests.cs
Task T020: Add batch-shape/eager/non-overlap cases in NativeBatchingConfigurationTests.cs
```

## Implementation Strategy

### MVP First — User Story 1

1. Complete Setup tasks T001–T003.
2. Complete shared test infrastructure T004–T006.
3. Write failing User Story 1 tests T007–T008.
4. Implement native retry/recovery tasks T009–T011.
5. Stop and validate T012 independently.

The MVP proves that failures no longer disappear and that native Serilog retries and recovers without duplicate accepted events. It intentionally does not yet close listener/fallback lifecycle coverage, consumer documentation, or package removal.

### Incremental Delivery

1. **Baseline + Test Foundation**: Protect current behavior and create deterministic test control.
2. **User Story 1**: Deliver native retry and recovery as the operational MVP.
3. **User Story 2**: Deliver standard listener/fallback and shutdown failure routing.
4. **User Story 3**: Deliver full configuration/source/package compatibility and remove the dependency.
5. **Polish**: Run scoped all-target validation, inspect the package, and prove the integration boundary remained untouched.

### Parallel Team Strategy

After Setup and Foundation:

1. Complete User Story 1 because it supplies the shared native wrapper.
2. Then work on User Story 2 tests/behavior and User Story 3 tests/docs in parallel where file ownership permits.
3. Serialize edits to `OpenSearchSink.cs` and `LoggerConfigurationOpenSearchExtensions.cs` to avoid conflicting ownership.
4. Join for package, multi-target, and protected-boundary validation.

## Notes

- Every checkbox remains unchecked until that exact implementation and validation is complete.
- `[P]` tasks modify different files or perform independent read-only checks.
- Do not run solution-wide restore/build/test commands because the solution includes the deferred integration project.
- Do not edit, format, clean, stage, or otherwise process `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` or its existing user-owned changes.
- Existing integration-project and solution changes must remain intact and must not be attributed to this feature.
- Do not add fixed scheduler sleeps or exact retry-time assertions; use synchronization gates and generous deadlock guards.
- Commit only after logical task groups if requested; task completion does not imply staging or committing.
