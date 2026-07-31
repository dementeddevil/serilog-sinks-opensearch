# Contract: Native Batching, Failure, and Compatibility

## Purpose

This contract defines the public configuration, registration, delivery, failure, and lifecycle behavior of the non-durable OpenSearch sink after migration to Serilog 4.4 native batching. Durable file shipping and live integration testing remain outside this contract except for regression protection.

## Public Configuration Contract

`OpenSearchSinkOptions` exposes:

```csharp
public TimeSpan? RetryTimeLimit { get; set; }
```

| Value | Required behavior |
|---|---|
| `null` | Use Serilog's native ten-minute retry default. |
| `TimeSpan.Zero` | Accept configuration; the first failed attempt exhausts retry. |
| Positive | Retry failed batches for the configured maximum duration. |
| Negative | Reject when constructing the sink/logger before accepting events. |

Existing batching values map as follows:

| Public option | Native behavior |
|---|---|
| `BatchPostingLimit` | Maximum events per batch. |
| `Period` | Maximum normal buffering interval. |
| `QueueSizeLimit = -1` | Unbounded queue. |
| `QueueSizeLimit > 0` | Bounded queue. |
| `QueueSizeLimit = 0` or `< -1` | Rejected configuration. |
| Existing eager behavior | First event emits eagerly. |

The options-object overload remains the primary configuration surface. The long named-argument overload may append nullable `retryTimeLimit` so XML/JSON configuration can set it without changing existing source calls.

## Registration Contract

### Non-durable

`WriteTo.OpenSearch(options)` registers the internal OpenSearch batch transport through Serilog's native batched sink overload, applying the existing minimum level and level switch before enqueueing.

### Durable

When `BufferBaseFilename` selects durable mode, registration continues through the existing durable `ILogEventSink`. `RetryTimeLimit` and native in-memory queue settings do not replace or wrap durable shipping.

## Public Sink Compatibility Contract

The following source patterns remain supported:

```csharp
using var sink = new OpenSearchSink(options);
ILogEventSink eventSink = sink;
sink.Emit(logEvent);
```

The public sink forwards:

- `Emit(LogEvent)` to the native batching wrapper;
- `Dispose()` to native final flushing;
- `SetFailureListener(ILoggingFailureListener)` to the native wrapper.

Assignment or casts to `Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink` are intentionally unsupported after migration. Binary compatibility with already compiled consumers of the old base type is not promised; documented source compatibility is the acceptance boundary.

## Batch Delivery Contract

The internal batch operation receives a non-empty `IReadOnlyCollection<LogEvent>` and:

1. Builds and submits one OpenSearch bulk request.
2. Returns successfully only for a valid completed response.
3. For a valid completed response with item errors, invokes configured legacy handlers only for rejected items and returns successfully.
4. Propagates every thrown whole-batch failure regardless of `EmitEventFailureHandling.ThrowException`.
5. Propagates the response's original exception when an unsuccessful response provides one.
6. Throws a safe diagnostic exception when an unsuccessful response has no original exception.
7. Never invokes legacy failure sinks/callbacks for whole-batch failures.
8. Never requests a whole-batch retry for a valid partial response, preventing duplicate accepted items.

`OnEmptyBatchAsync()` completes successfully and performs no additional work.

## Native Failure Contract

| Situation | Standard failure kind | Retry/fallback outcome |
|---|---|---|
| Batch attempt throws | Temporary | Same batch retained and scheduled; fallback not invoked. |
| Retry policy drops current batch/backlog | Permanent | Affected events reported and eligible for fallback. |
| Shutdown attempt fails | Permanent for current batch | Current events reported and eligible for fallback. |
| Shutdown abandons remaining queue | Final | Remaining events reported and eligible for fallback. |
| Queue is full | No notification in Serilog 4.4 | Event is silently discarded; no fallback guarantee. |

Every successful batch resets the native failure scheduler. The sink adds no retry classifier, retry loop, second queue, or synthetic overflow event.

## Disposal Contract

- Events accepted before disposal are delivered or at least attempted.
- Disposal delegates to Serilog's synchronous native batching disposal for the .NET Standard 2.0 target.
- Shutdown drains queued batches without normal retry scheduling.
- The first failed shutdown batch stops further OpenSearch attempts and causes applicable permanent/final notifications.
- Disposing the public façade also disposes the batched target when supported by native Serilog.

## Legacy Item-Failure Contract

`EmitEventFailure`, `FailureSink`, and `FailureCallback` remain applicable only when OpenSearch returns a valid completed bulk response containing rejected items. Existing SelfLog diagnostics and containment of exceptions thrown by these handlers remain. These settings do not suppress, duplicate, or reroute native whole-batch retries.

## Deferred Integration Boundary

- No file under `test/Serilog.Sinks.OpenSearch.IntegrationTests` is modified.
- No command in this contract restores, builds, runs, or claims validation of that project.
- Live cluster recovery, authentication, throttling, and shutdown validation require a separate feature.
