# Implementation Plan: Adopt Native Serilog Batching

**Branch**: `dev` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-native-batching/spec.md`

## Summary

Replace the non-durable sink's `Serilog.Sinks.PeriodicBatching` wrapper with the batching implementation built into Serilog 4.4. Register `BatchedOpenSearchSink` through the native batched `WriteTo.Sink` overload, propagate every invalid whole-batch outcome so Serilog owns retry, failure classification, fallback routing, and final flushing, and add a nullable `RetryTimeLimit` option that selects Serilog's ten-minute default when omitted. Preserve direct construction, `Emit()`, disposal, `ILogEventSink`, and failure-listener behavior through a public `OpenSearchSink` façade built with `LoggerSinkConfiguration.CreateSink()`. Keep valid-response bulk-item rejection handling and durable file shipping unchanged, remove the compatibility package, and exclude the integration-test project from all implementation and validation work.

## Technical Context

**Language/Version**: C# with `LangVersion=latest`; production targets .NET Standard 2.0 and is built with the repository-pinned .NET 10.0.302 SDK

**Primary Dependencies**: Serilog 4.4.0, OpenSearch.Net 2.0.0, Serilog.Formatting.Compact 3.0.0, and Serilog.Sinks.File 7.0.0; remove Serilog.Sinks.PeriodicBatching 5.0.0

**Storage**: Non-durable mode uses Serilog's in-memory batching queue; durable mode retains its existing rolling files and shipper unchanged

**Testing**: TUnit 1.63.0 with TUnit Assertions on Microsoft Testing Platform across .NET 8.0, .NET 9.0, and .NET 10.0; scripted `IConnection` doubles, synchronization gates, recording failure listeners, and collecting fallback sinks

**Target Platform**: Strong-named .NET Standard library consumed by supported .NET applications on Windows and Linux

**Project Type**: Multi-project .NET logging library with one in-scope unit-test project and a separately deferred external-service integration-test project

**Performance Goals**: Preserve configured batch partitioning, two-second default buffering cadence, eager first-event delivery, and single non-overlapping batch callbacks; prevent duplicate delivery of accepted items. No wall-clock throughput threshold is introduced because scheduling is owned by Serilog and CI timing would make it unstable.

**Constraints**: Serilog must be the sole non-durable queue/retry owner; every whole-batch failure is retryable until `RetryTimeLimit`; native queue overflow remains silent in Serilog 4.4 and must be documented; direct public sink construction/emit/disposal remains source-compatible; only periodic-base-class assignability may break; durable behavior is unchanged; warnings are errors; no files or commands under `test/Serilog.Sinks.OpenSearch.IntegrationTests` are in scope

**Scale/Scope**: One production sink project, one sample, README/changelog/package metadata, and the existing TUnit unit project. The recorded pre-feature unit baseline is 64 discovered cases per target (59 passed and 5 intentionally skipped), to be recaptured before implementation and expanded by the new batching cases.

## Constitution Check

*GATE: Passed before Phase 0 and re-checked after Phase 1.*

The repository constitution is an unratified placeholder and defines no enforceable gates. The clarified feature specification supplies the operative constraints.

| Gate | Pre-research | Post-design | Evidence |
|---|---|---|---|
| Native Serilog owns non-durable batching | PASS | PASS | The design registers `IBatchedLogEventSink` through native `BatchingOptions`; the compatibility façade delegates to the same native wrapper and introduces no queue. |
| Whole-batch failure ownership is unambiguous | PASS | PASS | All invalid whole-batch outcomes throw; native listeners/fallbacks own them, while legacy handlers remain valid-response item-level only. |
| Public source compatibility is bounded | PASS | PASS | The public type, constructor, `Emit`, `Dispose`, `ILogEventSink`, and listener forwarding remain; only the former compatibility base type is removed. |
| Durable behavior remains isolated | PASS | PASS | The durable branch retains its current sink, rolling files, shipper, backoff, and disposal lifecycle; only a stale documentation reference is removed. |
| Integration work is excluded | PASS | PASS | All artifacts and validation commands target production, sample, and unit projects explicitly and protect the dirty integration-test directory. |
| No unresolved technical clarification remains | PASS | PASS | Research resolves API shape, option mapping, retry/failure behavior, queue semantics, disposal, testing, and documentation choices. |

## Project Structure

### Documentation (this feature)

```text
specs/002-native-batching/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── native-batching.md
└── tasks.md                    # Created later by /speckit-tasks
```

### Source Code (repository root)

```text
src/Serilog.Sinks.OpenSearch/
├── Serilog.Sinks.OpenSearch.csproj
├── LoggerConfigurationOpenSearchExtensions.cs
└── Sinks/OpenSearch/
    ├── OpenSearchSink.cs
    ├── OpenSearchSinkOptions.cs
    └── Durable/ExponentialBackoffConnectionSchedule.cs  # Documentation-only cleanup

test/Serilog.Sinks.OpenSearch.Tests/
├── Serilog.Sinks.Opensearch.Tests.csproj
├── NativeBatchingOptionsTests.cs
├── NativeBatchingDeliveryTests.cs
├── NativeBatchingFailureTests.cs
├── NativeBatchingLifecycleTests.cs
└── Stubs/
    ├── ScriptedConnection.cs
    ├── RecordingFailureListener.cs
    └── CollectingSink.cs

test/Serilog.Sinks.OpenSearch.IntegrationTests/          # Explicitly excluded

sample/Serilog.Sinks.OpenSearch.Sample/
├── Program.cs
└── appsettings.json

README.md
CHANGES.md
AGENTS.md
```

**Structure Decision**: Modify the existing sink and unit project in place. Keep `BatchedOpenSearchSink` as the shared OpenSearch transport and static bulk-action helper so durable readers/clients require no behavioral change. Add narrowly named TUnit coverage and deterministic test doubles to the unit project. Do not run solution-wide commands because the solution includes the deferred integration project and that directory contains user-owned changes.

## Implementation Strategy

### A. Establish a protected baseline

- Record the current production/sample/unit-project build state and unit discovery/outcomes per target without invoking the solution or integration project.
- Record the existing public `OpenSearchSink` constructor, `Emit`, `Dispose`, `ILogEventSink`, level restriction, and level-switch usage as source-compatibility checks.
- Capture the integration directory's existing diff state only as a protected-boundary reference; do not inspect, edit, restore, build, or test that project.

### B. Introduce native batching configuration

- Add nullable `OpenSearchSinkOptions.RetryTimeLimit`; `null` leaves Serilog's ten-minute default, zero is valid, positive values map directly, and negative values fail during sink/logger construction.
- Centralize mapping from `BatchPostingLimit`, `Period`, eager-first-event, `QueueSizeLimit`, and `RetryTimeLimit` into native `BatchingOptions`.
- Correct queue validation so `-1` means unbounded, positive values mean bounded, and zero or values below `-1` are rejected. This realizes the existing but currently unreachable `-1` mapping.
- Append an optional nullable retry-time argument to the long configuration overload so named XML/JSON configuration can opt in without breaking existing source calls; the options object remains the primary contract.

### C. Replace the compatibility wrapper

- Change `BatchedOpenSearchSink` to Serilog 4.4's `IBatchedLogEventSink` signature using `IReadOnlyCollection<LogEvent>` and retain `OnEmptyBatchAsync()`.
- In `WriteTo.OpenSearch`, register non-durable mode through `Sink(IBatchedLogEventSink, BatchingOptions, minimumLevel, levelSwitch)`; retain ordinary `ILogEventSink` registration for durable mode.
- Replace `OpenSearchSink : PeriodicBatchingSink` with an `ILogEventSink`, `IDisposable`, and `ISetLoggingFailureListener` façade whose constructor uses `LoggerSinkConfiguration.CreateSink()` to obtain the same native batching wrapper. Delegate `Emit`, disposal/final flush, and failure-listener attachment.
- Remove the periodic-batching package, namespace usage, base-class constructor, and stale XML documentation while leaving durable scheduling behavior intact.

### D. Make failure ownership explicit

- Remove whole-batch catch-and-consume handling. Let transport, payload, serialization, request, protocol, and other exceptions propagate regardless of `ThrowException`.
- For `Success == false`, propagate the associated original exception or synthesize a payload-free diagnostic exception containing safe response context when no original exception exists.
- Preserve existing legacy SelfLog/failure-sink/callback behavior only for rejected items in a valid completed bulk response; contain exceptions thrown by those legacy handlers as before.
- Let Serilog report temporary attempts, permanent losses after retry exhaustion, and final queued losses during shutdown. Do not add a competing classifier, retry loop, queue, or overflow signal.

### E. Prove behavior deterministically

- Add pure option-mapping and validation tests, including native defaults, zero/negative retry, positive/unbounded queues, and the corrected invalid queue values.
- Test direct batched transport propagation and completed partial-bulk item handling with scripted responses.
- Test recovery, retry exhaustion, failure listeners, fallback chains, success reset, eager emission, batch partitioning, and disposal/final flush through a real Serilog logger and synchronization-gated connection doubles.
- Use `TaskCompletionSource`/`SemaphoreSlim` gates and generous deadlock timeouts; do not assert exact scheduler milliseconds or rely on fixed sleeps.
- Re-run the complete unit suite across all three targets three times, reconcile the pre-feature baseline plus new cases, build the sample as a consumer, pack the sink, inspect dependencies, and audit compatibility API references.

### F. Update consumer guidance

- Document `RetryTimeLimit`, its native ten-minute default, zero behavior, option/configuration examples, native retry/failure-listener/fallback behavior, shutdown handling, and the Serilog 4.4 bounded-queue overflow limitation.
- Rewrite failure guidance to distinguish whole-batch native failures from valid-response item rejections handled by `EmitEventFailure` options.
- Update the sample and `[Unreleased]` changelog entry, keeping durable guidance unchanged.

## Deferred Work Boundary

`test/Serilog.Sinks.OpenSearch.IntegrationTests` requires separate specification, planning, and implementation. This feature makes no framework, dependency, cluster-lifecycle, target, solution, or CI decision for it. Existing changes in that directory belong to the user and must remain untouched.
