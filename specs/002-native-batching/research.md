# Research: Adopt Native Serilog Batching

## Decision 1: Use the Serilog 4.4 native batched sink contract

**Decision**: Keep `BatchedOpenSearchSink` as the transport, implement Serilog 4.4's `IBatchedLogEventSink` using `EmitBatchAsync(IReadOnlyCollection<LogEvent>)` and `OnEmptyBatchAsync()`, and register non-durable mode with `WriteTo.Sink(IBatchedLogEventSink, BatchingOptions, minimumLevel, levelSwitch)`.

**Rationale**: Serilog explicitly instructs batched sink implementations to propagate exceptions so its infrastructure can own diagnostics, retries, listener notification, fallback routing, flushing, and disposal. The repository already references Serilog 4.4.0 and .NET Standard 2.0, so no additional runtime dependency is required.

**Alternatives considered**:

- Retain `Serilog.Sinks.PeriodicBatching`: rejected because it is compatibility-only for Serilog 4 projects and blocks native failure handling when exceptions are consumed.
- Add a new sink-owned scheduler or queue: rejected because it creates competing lifecycle and failure ownership.
- Add cancellation-token parameters: rejected because Serilog 4.4's interface does not expose them.

**Sources**: [Serilog 4.4 `IBatchedLogEventSink`](https://github.com/serilog/serilog/blob/497f80fda4f9e8f98b9c13ba34b1f0530f8c4449/src/Serilog/Core/IBatchedLogEventSink.cs), [periodic batching migration guidance](https://github.com/serilog/serilog-sinks-periodicbatching)

## Decision 2: Preserve direct public sink usage with a native façade

**Decision**: Make public `OpenSearchSink` implement `ILogEventSink`, `IDisposable`, and `ISetLoggingFailureListener`. Its existing constructor uses `LoggerSinkConfiguration.CreateSink()` with the native batched overload, then delegates `Emit`, disposal, and listener attachment to the resulting native batching wrapper. The normal extension path registers `BatchedOpenSearchSink` directly to avoid an unnecessary façade layer.

**Rationale**: `CreateSink()` is the public Serilog 4.4 mechanism for constructing configured sinks outside a logger pipeline. It returns the native internal `BatchingSink` through `ILogEventSink`; that object also supports disposal and failure-listener attachment. This preserves current `new OpenSearchSink(options)`, `sink.Emit(...)`, `using`, `ILogEventSink`, and fallback/listener source patterns while accepting only the clarified periodic-base-class assignability break.

**Alternatives considered**:

- Make public `OpenSearchSink` only an `IBatchedLogEventSink`: rejected because inherited `Emit`, `Dispose`, `ILogEventSink`, and current direct-use source patterns would also break.
- Construct an inner `Logger`: rejected because it adds an unnecessary logging pipeline/minimum-level layer and hides post-construction failure-listener attachment.
- Retain the periodic base class as an adapter: rejected because the package dependency would remain.

**Source**: [Serilog 4.4 `LoggerSinkConfiguration`](https://github.com/serilog/serilog/blob/497f80fda4f9e8f98b9c13ba34b1f0530f8c4449/src/Serilog/Configuration/LoggerSinkConfiguration.cs)

## Decision 3: Map existing options once and expose nullable retry configuration

**Decision**: Use one internal mapper with these rules:

| OpenSearch option | Native option |
|---|---|
| `BatchPostingLimit` | `BatchSizeLimit` |
| `Period` | `BufferingTimeLimit` |
| existing eager behavior | `EagerlyEmitFirstEvent = true` |
| `QueueSizeLimit == -1` | `QueueLimit = null` |
| positive `QueueSizeLimit` | bounded `QueueLimit` |
| `RetryTimeLimit == null` | retain native ten-minute default |
| non-null `RetryTimeLimit` | native `RetryTimeLimit` |

Add `TimeSpan? RetryTimeLimit` to `OpenSearchSinkOptions`, and append an optional nullable retry argument to the long named-argument configuration overload. Null uses Serilog's default, zero causes exhaustion on the first failed attempt, positive values bound retries, and negative values fail during sink construction.

**Rationale**: Nullable configuration distinguishes “use the SDK default” from an explicit zero and prevents duplicating Serilog's default value in the sink. Extending the existing named configuration path makes the option usable from code and configuration while preserving existing source calls.

**Alternatives considered**:

- Non-null property initialized to ten minutes: rejected because it duplicates the SDK default and cannot distinguish omission.
- Configure retry only on the long overload: rejected because the options object is the primary public contract.
- Add a sink-specific retry policy object: rejected because the native scheduler owns retry behavior.

**Source**: [Serilog 4.4 `BatchingOptions`](https://github.com/serilog/serilog/blob/497f80fda4f9e8f98b9c13ba34b1f0530f8c4449/src/Serilog/Configuration/BatchingOptions.cs)

## Decision 4: Correct the queue sentinel contract

**Decision**: Accept `QueueSizeLimit = -1` as unbounded, accept positive bounds, and reject zero or values below `-1` at option assignment or construction.

**Rationale**: `OpenSearchSink` already contains a `-1` to `null` mapping, but the current public setter rejects all negative values and accepts zero. The clarified spec requires making the intended unbounded sentinel reachable and giving invalid values deterministic validation.

**Alternatives considered**:

- Remove the `-1` sentinel: rejected because it contradicts the specification and existing mapping intent.
- Treat zero as unbounded: rejected because it conflicts with native semantics and the existing documented 100,000 default.
- Defer all validation to Serilog: rejected because the sink's public option should describe its own sentinel clearly.

## Decision 5: Propagate every invalid whole-batch outcome

**Decision**: Remove the whole-batch catch/consume path. Propagate thrown transport, connection, payload, serialization, request, and protocol failures. For an unsuccessful response, propagate `OriginalException` when present; otherwise throw a new diagnostic exception without including event payloads. Do not consult `EmitEventFailureHandling.ThrowException` for whole-batch outcomes.

**Rationale**: Native batching has one failed-batch signal: an exception from `EmitBatchAsync`. It reports the attempt as temporary, retains the same batch, and uses its scheduler until retry exhaustion. Selective sink classification would compete with the clarified native ownership model.

**Alternatives considered**:

- Retry only selected status codes: rejected by clarification; it also requires a competing classifier.
- Invoke legacy failure handlers before rethrowing: rejected because it duplicates events before the retry result is known.
- Return success for unsuccessful responses without exceptions: rejected because it silently loses the batch.

## Decision 6: Retain legacy handling only for completed item rejections

**Decision**: A valid completed bulk response with rejected individual items returns batch-level success. Invoke the existing SelfLog, failure sink, and callback only for rejected items; contain errors thrown by those handlers as today. Never invoke them for a whole-batch failure and never resend items already accepted in that response.

**Rationale**: OpenSearch can accept and reject different items in one completed bulk request. Retrying the whole response would duplicate accepted events, while calling legacy handlers on temporary whole-batch attempts would race native retries and fallbacks.

**Alternatives considered**:

- Retry only rejected bulk items: rejected because it changes the established per-item contract and requires reconstructing batches.
- Throw the entire completed response when any item fails: rejected because it resends successful items.
- Remove legacy handlers: rejected because valid-response item behavior remains a supported source contract.

## Decision 7: Use native retry, listener, fallback, and shutdown semantics

**Decision**: Accept Serilog 4.4's scheduler and failure categories unchanged:

- Each thrown attempt is reported `Temporary` and retained for retry.
- Exhausted batches and drained backlog are reported `Permanent`; a success resets scheduler failure state.
- Fallback chains ignore `Temporary` and forward `Permanent`/`Final` events.
- Disposal completes the queue and attempts remaining batches without retry; a shutdown emission failure reports the current batch `Permanent`, remaining queued events `Final`, and stops.
- The .NET Standard 2.0 public façade supports synchronous `IDisposable`, matching the current public sink surface.

**Rationale**: These are the standard behaviors the migration is intended to unlock. Reimplementing or translating them would weaken compatibility with Serilog failure listeners and fallback chains.

**Alternatives considered**:

- Add sink-specific retry intervals: rejected because `BatchingOptions` and Serilog's failure-aware scheduler already define them.
- Invoke fallback on every temporary attempt: rejected because native fallback forwarding intentionally waits for permanent/final loss.
- Add a sink-specific shutdown timeout: rejected because it is not part of the native 4.4 contract or the feature request.

**Sources**: [native batching implementation](https://github.com/serilog/serilog/blob/497f80fda4f9e8f98b9c13ba34b1f0530f8c4449/src/Serilog/Core/Sinks/Batching/BatchingSink.cs), [failure-aware scheduler](https://github.com/serilog/serilog/blob/497f80fda4f9e8f98b9c13ba34b1f0530f8c4449/src/Serilog/Core/Sinks/Batching/FailureAwareBatchScheduler.cs), [fallback forwarding](https://github.com/serilog/serilog/blob/497f80fda4f9e8f98b9c13ba34b1f0530f8c4449/src/Serilog/Core/Sinks/Fallback/DelegatingLoggingFailureListener.cs)

## Decision 8: Accept and document silent native queue overflow

**Decision**: Use Serilog's native bounded channel without a second queue. Document that Serilog 4.4 ignores a failed `TryWrite()` when the queue is full and emits no failure-listener notification or fallback delivery for that overflow.

**Rationale**: The clarification explicitly accepts this SDK limitation. A second queue would undermine native ownership and change memory/ordering behavior.

**Alternatives considered**:

- Block the migration: rejected by clarification.
- Add a sink-owned overflow queue or signal: rejected by clarification and because native `BatchingSink` provides no interception point.
- Default to an unbounded queue: rejected because it changes memory safety and the established 100,000-event default.

## Decision 9: Keep durable shipping outside native non-durable batching

**Decision**: Preserve `DurableOpenSearchSink`, its file buffer, shipper, backoff, and disposal. `RetryTimeLimit` has no effect when `BufferBaseFilename` selects durable mode. Keep `BatchedOpenSearchSink` static bulk helpers used by durable code, and only replace the durable backoff class's stale XML reference to the removed package.

**Rationale**: Durable shipping already has a distinct persistence and retry lifecycle. Combining it with the native in-memory lifecycle would produce double buffering/retry and exceed scope.

**Alternatives considered**:

- Wrap durable mode in native batching: rejected because it weakens durability and adds competing retry state.
- Refactor shared bulk helpers during migration: rejected as unrelated risk.

## Decision 10: Validate with deterministic unit contracts, not live integration or throughput thresholds

**Decision**: Add scripted connection, failure-listener, and fallback doubles; coordinate retry/disposal cases with task/semaphore gates and generous deadlock guards; validate all unit targets, sample compilation, packed dependencies, and three repeat runs. Do not use fixed sleeps, exact backoff timing, a live cluster, solution-wide commands, or a wall-clock throughput release threshold.

**Rationale**: Native scheduler intervals include a five-second minimum backoff and CI load makes exact timings flaky. Observable state transitions, batch shapes, listener events, fallback deliveries, and disposal outcomes prove the contract deterministically. The integration project is explicitly deferred and currently contains user-owned changes.

**Alternatives considered**:

- Add live OpenSearch retry tests now: rejected because the integration project requires separate processing.
- Assert exact retry timestamps: rejected because scheduler/host timing is nondeterministic.
- Use a throughput benchmark as a merge gate: rejected because the migration delegates scheduler performance upstream; batch partitioning and stable repeated runs are the relevant regression gates.
