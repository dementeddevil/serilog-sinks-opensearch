# Tasks: Serialize Trace and Span Context

**Input**: Design documents from `specs/003-serialize-trace-context/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/trace-context-formatting.md`, `quickstart.md`

**Tests**: The specification explicitly requires activity/no-activity, inline/nested, collision, alternate-formatter, concurrency, compatibility, and performance coverage. Test tasks precede their corresponding implementation tasks.

**Organization**: Tasks are grouped by user story so automatic correlation, collision-safe property layouts, and configurable formatter ownership can be implemented and validated independently. The integration-test project and solution are protected exclusions throughout.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it changes a different file and has no dependency on another incomplete task
- **[Story]**: Maps the task to User Story 1, 2, or 3
- Every task identifies the exact file or directory it changes, creates, or validates

## Phase 1: Setup (Shared Evidence)

**Purpose**: Capture current product, test, package, and protected-boundary evidence before formatter changes.

- [X] T001 Create `specs/003-serialize-trace-context/validation-evidence.md` and record branch, SDK, scoped formatter/sink/unit/sample restore and Release-build outcomes, plus current unit discovery/pass/skip counts on `net8.0`, `net9.0`, and `net10.0` without invoking the solution
- [X] T002 [P] Capture the current user-owned state of `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` and `serilog-sinks-opensearch.sln` in `specs/003-serialize-trace-context/integration-boundary-baseline.md` using scoped status/diff evidence or reproducible source hashes without restoring, building, testing, or modifying the integration project
- [X] T003 [P] Record pre-change formatter constructors, default sink option/factory behavior, formatter/sink package versions, alternate-formatter selection, and packed dependency expectations in `specs/003-serialize-trace-context/compatibility-baseline.md` using `src/Serilog.Formatting.OpenSearch/**`, `src/Serilog.Sinks.OpenSearch/**`, `README.md`, and scoped baseline package inspection

**Checkpoint**: Baseline builds, tests, public surfaces, packages, and the excluded integration state are documented before implementation.

---

## Phase 2: Foundational (Deterministic Test Infrastructure)

**Purpose**: Add shared event, activity, parsing, and formatter-observation helpers used by every story.

**⚠️ CRITICAL**: Complete this phase before story-specific tests.

- [X] T004 [P] Add fixed trace/span ID creation, real logger capture inside/outside W3C activities, explicit/default `LogEvent` factories, and JSON/fragment parsing helpers in `test/Serilog.Sinks.OpenSearch.Tests/Stubs/TraceContextTestHelper.cs`
- [X] T005 [P] Add an `ITextFormatter` double that records exact `LogEvent` references and emits deterministic marker output in `test/Serilog.Sinks.OpenSearch.Tests/Stubs/RecordingTextFormatter.cs`

**Checkpoint**: Story tests can prove capture timing, exact identifiers, valid output, source-event identity, and absence of ambient leakage without live OpenSearch.

---

## Phase 3: User Story 1 - Preserve Distributed Trace Correlation (Priority: P1) 🎯 MVP

**Goal**: Built-in formatter documents contain canonical first-class trace/span identifiers by default and omit unavailable/default identifiers.

**Independent Test**: Format events captured inside and outside an activity plus explicit fixed/default-ID events; traced documents contain exact root identifiers and untraced/default documents contain neither placeholder field.

### Tests for User Story 1

> **NOTE**: Write these tests first and confirm the relevant assertions fail before production changes.

- [X] T006 [US1] Add failing TUnit cases for real logger capture inside/outside an activity, retained capture after activity stop, consecutive activities, exact 32/16 lowercase values, explicit fixed IDs, and null/default omission in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextFormattingTests.cs`
- [X] T007 [US1] Extend `test/Serilog.Sinks.OpenSearch.Tests/TraceContextFormattingTests.cs` with failing cases for independent trace/span availability, default-on constructor behavior, enclosing-object output, unenclosed fragments, custom closing delimiters, and `ExceptionAsObjectJsonFormatter`

### Implementation for User Story 1

- [X] T008 [US1] Add immutable default-on trace/span controls, event-owned nullable/default detection, and protected metadata writer hooks to `src/Serilog.Formatting.OpenSearch/DefaultJsonFormatter.cs` without reading ambient activity state
- [X] T009 [US1] Add public `TraceIdPropertyName`/`SpanIdPropertyName` constants, append optional trace/span constructor controls, and emit canonical root values in `src/Serilog.Formatting.OpenSearch/OpenSearchJsonFormatter.cs`
- [X] T010 [US1] Append and forward the same optional trace/span controls in `src/Serilog.Formatting.OpenSearch/ExceptionAsObjectJsonFormatter.cs`
- [X] T011 [US1] Run the `TraceContextFormattingTests` filter on `net8.0`, verify the complete/no-context/fragment/derived-formatter contract, and append exact command/count/outcome evidence to `specs/003-serialize-trace-context/validation-evidence.md`

**Checkpoint**: User Story 1 is complete when first-class captured context survives formatting by default without adding empty fields or consulting current ambient state.

---

## Phase 4: User Story 2 - Keep Documents Unambiguous Across Property Layouts (Priority: P2)

**Goal**: First-class IDs remain authoritative and unique in inline/nested documents while non-conflicting properties and original rendering behavior remain unchanged.

**Independent Test**: Format traced, untraced, and opted-out events with exact/mixed-case scalar and structured collision properties in both layouts; parse every document and verify one semantic ID maximum, correct precedence, property preservation, and unchanged rendered content.

### Tests for User Story 2

- [X] T012 [P] [US2] Add failing parsed-JSON TUnit matrices for nested/inline layouts, exact/lower/mixed-case trace/span collisions, scalar/structured collision values, first-class precedence, unavailable/disabled fallbacks, ordinary property retention, and all-properties-collide output in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextCollisionTests.cs`
- [X] T013 [P] [US2] Add failing TUnit cases proving colliding properties still participate in rendered messages and rendering metadata while suppressed only from document-property output in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextRenderingTests.cs`
- [X] T014 [P] [US2] Add a deterministic 10,000-event parallel mixed traced/untraced test with per-event JSON parsing and identifier correlation in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextConcurrencyTests.cs`

### Implementation for User Story 2

- [X] T015 [US2] Implement ordinal case-insensitive, emitted-field-conditional property filtering with original-dictionary reuse, collision-only allocation, no spurious empty property container, and unchanged rendering inputs in `src/Serilog.Formatting.OpenSearch/DefaultJsonFormatter.cs`
- [X] T016 [US2] Run `TraceContextCollisionTests`, `TraceContextRenderingTests`, and `TraceContextConcurrencyTests` on `net8.0`, verify layout/precedence/rendering/concurrency independently, and append exact results to `specs/003-serialize-trace-context/validation-evidence.md`

**Checkpoint**: User Story 2 is complete when built-in output contains no semantic duplicate IDs and collision handling changes neither event identity nor established rendering.

---

## Phase 5: User Story 3 - Control Adoption Without Changing Formatter Ownership (Priority: P3)

**Goal**: Direct formatter and default sink consumers receive independent opt-outs, existing source calls remain valid, and compact/custom formatters retain exclusive output ownership.

**Independent Test**: Compile and exercise default/trace-disabled/span-disabled/both-disabled formatter and sink configurations, then compare explicitly selected compact/custom output and exact event identity with their direct formatter behavior.

### Tests for User Story 3

- [X] T017 [P] [US3] Add failing TUnit cases for all formatter enablement combinations, existing constructor source calls, new named arguments, derived formatter forwarding, and public field-name constants in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextCompatibilityTests.cs`
- [X] T018 [P] [US3] Add failing TUnit cases for default-true sink options, normal/durable built-in factory propagation, appended named-configuration parameters, and existing configuration call compatibility in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextSinkOptionsTests.cs`
- [X] T019 [P] [US3] Add failing TUnit cases proving custom normal/durable formatters receive the same event reference and unmodified marker output, and explicitly selected compact output exactly matches direct compact formatting in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextFormatterOwnershipTests.cs`
- [X] T020 [P] [US3] Add a separately filterable Release benchmark harness with prebuilt events, warm-up, interleaved trials, raw sample reporting, median comparison, and idle-environment guard in `test/Serilog.Sinks.OpenSearch.Tests/TraceContextFormatterBenchmarkTests.cs`

### Implementation for User Story 3

- [X] T021 [US3] Add default-true `RenderTraceId` and `RenderSpanId` configuration with XML documentation in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/OpenSearchSinkOptions.cs`
- [X] T022 [US3] Pass trace/span settings into normal and durable built-in formatter factories while preserving `CustomFormatter`/`CustomDurableFormatter` selection exactly in `src/Serilog.Sinks.OpenSearch/Sinks/OpenSearch/OpenSearchSinkState.cs`
- [X] T023 [US3] Append named `renderTraceId`/`renderSpanId` parameters, XML documentation, and option mapping without changing existing calls in `src/Serilog.Sinks.OpenSearch/LoggerConfigurationOpenSearchExtensions.cs`
- [X] T024 [US3] Set the coordinated major release versions and preserve all other package metadata in `src/Serilog.Formatting.OpenSearch/Serilog.Formatting.OpenSearch.csproj` and `src/Serilog.Sinks.OpenSearch/Serilog.Sinks.OpenSearch.csproj`
- [X] T025 [P] [US3] Document default fields, canonical formats, omission, independent opt-outs, collision precedence, mapping review, and compact/custom ownership in `README.md`
- [X] T026 [P] [US3] Add default and opt-out usage without changing alternate/durable behavior in `sample/Serilog.Sinks.OpenSearch.Sample/Program.cs` and `sample/Serilog.Sinks.OpenSearch.Sample/appsettings.json`
- [X] T027 [P] [US3] Add formatter/sink major-version and trace-context behavior notes under `[Unreleased]` in `CHANGES.md`
- [X] T028 [US3] Run all `TraceContext*Tests` except the benchmark on `net8.0`, build the sample consumer, and append configuration/ownership/source-compatibility results to `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T029 [US3] Run `TraceContextFormatterBenchmarkTests` on an idle Release environment, record machine/runtime/raw samples/median ratio in `specs/003-serialize-trace-context/validation-evidence.md`, and leave SC-008 explicitly unverified if the guard, stability, or 95% threshold fails

**Checkpoint**: User Story 3 is complete when built-in settings are configurable across public surfaces, alternate formatter output is untouched, and coordinated release/package behavior is documented and measured.

---

## Phase 6: Polish & Cross-Cutting Validation

**Purpose**: Close the feature with all-target regression, package, performance, reference, requirement, and protected-boundary evidence.

- [X] T030 Execute the scoped restore and Release-build sequence from `specs/003-serialize-trace-context/quickstart.md` for formatter, sink, unit, and sample projects only, and append commands/exit codes/outcomes to `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T031 Run all non-benchmark `TraceContext*Tests` on `net8.0`, `net9.0`, and `net10.0`, recording per-target discovery and outcomes in `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T032 Run the complete unit suite on `net8.0`, `net9.0`, and `net10.0`, reconcile the captured baseline plus enumerated new cases and retained skips, and append exact counts to `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T033 Pack formatter and sink projects, inspect both generated `.nuspec` files, compile representative old/new public calls, and record proof of coordinated major versions and the formatter dependency in `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T034 Run the scoped trace-reference, ambient-activity, alternate-formatter, package-content, and whitespace audits from `specs/003-serialize-trace-context/quickstart.md`, classify legitimate test/docs references, and record results in `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T035 Compare `test/Serilog.Sinks.OpenSearch.IntegrationTests/**` and `serilog-sinks-opensearch.sln` with `specs/003-serialize-trace-context/integration-boundary-baseline.md`, preserve every user-owned change, and record whether zero feature-driven edits can be proven in `specs/003-serialize-trace-context/validation-evidence.md`
- [X] T036 Reconcile FR-001 through FR-020 and SC-001 through SC-008 against actual implementation/evidence, leave failed or unavailable criteria explicitly incomplete, and record any approved exception in `specs/003-serialize-trace-context/validation-evidence.md`

---

## Phase 7: User-Authorized Live Integration Follow-up

**Purpose**: Verify the implemented document contract after indexing through the real sink against the existing OpenSearch 1.x and 2.x Testcontainers fixtures.

- [X] T037 Add a reusable live scenario in `test/Serilog.Sinks.OpenSearch.IntegrationTests/TraceContextIntegrationScenarios.cs` covering default traced/untraced output, canonical event-owned identifiers, nested collision fallback, inline fields, and independent trace opt-out
- [X] T038 Register the live scenario for both existing version fixtures in `test/Serilog.Sinks.OpenSearch.IntegrationTests/OpenSearchVersionTests.cs` and document the expanded matrix in `test/Serilog.Sinks.OpenSearch.IntegrationTests/README.md`
- [X] T039 Restore and Release-build the integration project, run its generated TUnit executable when Docker is available, and append exact outcomes to `specs/003-serialize-trace-context/validation-evidence.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 — Setup**: No dependencies; T001, T002, and T003 can begin together
- **Phase 2 — Foundation**: Depends on baseline capture; T004 and T005 can run in parallel
- **Phase 3 — US1**: Depends on Foundation; tests T006-T007 precede production T008-T010, followed by T011
- **Phase 4 — US2**: Uses US1 metadata emission; tests T012-T014 precede collision implementation T015, followed by T016
- **Phase 5 — US3**: Uses US1 formatter controls; tests T017-T020 precede T021-T024, docs/sample/changelog T025-T027 can then proceed in parallel, followed by T028-T029
- **Phase 6 — Polish**: Depends on all desired stories; final boundary and requirement reconciliation occur last

### User Story Dependencies

- **US1 (P1)**: Starts after shared helpers and has no story dependency; this is the MVP
- **US2 (P2)**: Depends on US1 first-class emission so collision precedence can be observed, but has its own independent layout/rendering/concurrency tests
- **US3 (P3)**: Depends on US1 controls and integrates them into public sink/alternate formatter/release surfaces; it remains independently testable through configuration and ownership tests

### Within Each User Story

- Write tests and confirm the intended assertions fail before production changes
- Preserve same-file serialization: `DefaultJsonFormatter.cs` changes occur in US1 before US2
- Complete focused story validation before moving to the next priority
- Record exact evidence; do not generalize nearby passing builds/tests to an unrun task

## Parallel Opportunities

- T001-T003 are independent baseline artifacts/checks
- T004-T005 create separate helper files
- T012-T014 cover collision, rendering, and concurrency in separate test files
- T017-T020 cover public compatibility, sink options, formatter ownership, and performance in separate test files
- T025-T027 update README, sample, and changelog independently after public behavior is final

## Parallel Example: User Story 1

```text
Task T006: Add activity/fixed/default identifier cases in TraceContextFormattingTests.cs
Task T007: Extend the same test file after T006 with fragment/configuration/derived cases
```

US1 test tasks share a file and are intentionally sequential.

## Parallel Example: User Story 2

```text
Task T012: Add property-layout collision matrices in TraceContextCollisionTests.cs
Task T013: Add original-property rendering cases in TraceContextRenderingTests.cs
Task T014: Add 10,000-event safety cases in TraceContextConcurrencyTests.cs
```

## Parallel Example: User Story 3

```text
Task T017: Add public formatter compatibility cases in TraceContextCompatibilityTests.cs
Task T018: Add sink option/factory/configuration cases in TraceContextSinkOptionsTests.cs
Task T019: Add custom/compact ownership cases in TraceContextFormatterOwnershipTests.cs
Task T020: Add controlled benchmark harness in TraceContextFormatterBenchmarkTests.cs
```

## Implementation Strategy

### MVP First — User Story 1

1. Complete baseline and helpers (T001-T005).
2. Write failing correlation contracts (T006-T007).
3. Implement first-class metadata output (T008-T010).
4. Validate independently (T011).

The MVP stops discarding captured trace context without yet adding collision precedence, sink opt-outs, alternate-formatter proofs, packaging, or performance evidence.

### Incremental Delivery

1. **Baseline + Foundation**: Protect current behavior and enable deterministic tests.
2. **US1**: Deliver automatic canonical trace correlation.
3. **US2**: Deliver unique collision-safe documents and concurrency proof.
4. **US3**: Deliver opt-outs, ownership compatibility, coordinated releases, docs, and benchmark evidence.
5. **Polish**: Complete all-target, package, audit, requirements, and protected-boundary evidence.

## Notes

- Every checkbox remains unchecked until that exact task is completed.
- `[P]` tasks use different files or independent read-only checks.
- Never restore, build, test, modify, or claim validation of `test/Serilog.Sinks.OpenSearch.IntegrationTests/**`.
- Never invoke `serilog-sinks-opensearch.sln`; use project-scoped commands only.
- Benchmark evidence is separate from ordinary unit timing and cannot be inferred from correctness passes.
