# Feature Specification: Adopt Native Serilog Batching

**Feature Branch**: `dev`

**Created**: 2026-07-31

**Updated**: 2026-07-31

**Status**: Draft

**Input**: User description: "Replace Serilog.Sinks.PeriodicBatching with Serilog's native batching; remove the compatibility dependency, expose a configurable retry time limit, integrate with Serilog failure listeners and fallback chains, simplify disposal and final flushing, and allow batch transport failures to propagate so native retry and failure handling can operate."

## Clarifications

### Session 2026-07-31

- Q: What compatibility contract should apply to direct use of `OpenSearchSink`? → A: Preserve the public type and constructor; only former base-class assignability may break.
- Q: Which whole-batch failures should native batching retry? → A: Retry all whole-batch failures until `RetryTimeLimit` is exhausted.
- Q: How should queue-overflow observability affect this migration? → A: Use native queue behavior and document that current Serilog versions do not emit an overflow notification.
- Q: How should legacy failure handlers apply to whole-batch failures? → A: Legacy handlers remain item-level only; native listeners and fallbacks exclusively handle whole-batch failures.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Recover from Temporary OpenSearch Delivery Failures (Priority: P1)

As an operator, I want temporarily failed log batches to be retried for a bounded period so short OpenSearch or network outages do not immediately lose diagnostic events.

**Why this priority**: The current sink usually consumes batch transport exceptions, which makes a failed delivery appear successful and prevents the logging pipeline from applying its retry policy.

**Independent Test**: Configure a short retry window, make consecutive batch attempts fail before a later successful attempt, and verify that the same batch is retried and delivered once the simulated transport recovers.

**Acceptance Scenarios**:

1. **Given** a batch transport request fails temporarily, **When** the configured retry window has not elapsed, **Then** the failure is surfaced to the batching owner and the batch is scheduled for another attempt.
2. **Given** one or more temporary failures followed by recovery within the retry window, **When** the next attempt succeeds, **Then** the batch is accepted and normal batching resumes.
3. **Given** failures continue beyond the retry window, **When** the batching owner determines that buffered events must be dropped, **Then** the affected events are reported through the standard failure path as permanent losses.
4. **Given** the retry window is configured as zero, **When** a batch fails, **Then** no extended retry period is applied and the standard failure path reports the resulting loss.

---

### User Story 2 - Observe and Route Batching Failures Consistently (Priority: P2)

As an application owner, I want OpenSearch batching failures to participate in the same failure-listener and fallback-chain behavior as other Serilog sinks so I can route undelivered events and diagnose temporary, permanent, and shutdown failures consistently.

**Why this priority**: A retry policy is operationally useful only when failures and eventual losses remain visible to the logging pipeline and its configured fallback destinations.

**Independent Test**: Attach a recording failure listener and fallback destination, induce retryable failure, retry exhaustion, and shutdown failure, and verify the standard failure kinds, affected events, and fallback deliveries without duplicate sink-specific handling for each retry attempt.

**Acceptance Scenarios**:

1. **Given** a failure listener is configured, **When** a batch attempt throws, **Then** the listener receives the standard temporary-failure notification with the affected batch and exception.
2. **Given** the retry policy drops a failed batch or queued events, **When** the loss becomes permanent, **Then** the listener and any configured fallback chain receive the affected events through the standard permanent-failure path.
3. **Given** a batch cannot be emitted while the sink is shutting down, **When** remaining events are abandoned, **Then** the standard final-failure path reports the affected events.
4. **Given** no custom failure listener is configured, **When** batching fails, **Then** the standard Serilog diagnostic path remains available.

---

### User Story 3 - Configure and Operate Batching without a Compatibility Package (Priority: P3)

As a maintainer, I want the non-durable OpenSearch sink to use the batching capability already supplied by the supported Serilog version so the extension has fewer dependencies and one clear owner for queueing, retry scheduling, flushing, and disposal.

**Why this priority**: Removing the compatibility layer reduces maintenance, but it must preserve the public configuration flow and established batching behavior relied upon by existing applications.

**Independent Test**: Build and package the sink without the compatibility package, configure it through the existing OpenSearch registration flow, and verify batch size, buffering period, queue limit, eager first emission, retry time, level restriction, flushing, and disposal behavior.

**Acceptance Scenarios**:

1. **Given** an existing non-durable OpenSearch configuration, **When** it is used after the migration without specifying a retry time, **Then** its established batching settings continue to apply and the documented default retry window is used.
2. **Given** an application specifies a retry time limit, **When** its logger is created, **Then** that value controls the maximum period for retrying failed batches.
3. **Given** queued events remain when the logger is disposed, **When** disposal completes, **Then** delivery is attempted for all events accepted before disposal and any undeliverable remainder is reported through the standard failure path.
4. **Given** the sink package is restored by a consumer, **When** its dependency graph is inspected, **Then** the periodic-batching compatibility package is absent.

### Edge Cases

- OpenSearch returns an unsuccessful batch response with an associated transport exception rather than throwing it directly.
- OpenSearch returns an unsuccessful batch response without an associated exception.
- A bulk request succeeds at the transport level but contains one or more rejected individual items alongside successful items.
- The transport recovers immediately before or immediately after the configured retry window is exhausted.
- The configured retry time limit is zero, negative, or extremely large.
- The queue is configured as unbounded, reaches its configured capacity, or receives events while disposal is in progress.
- Disposal begins while a failed batch is waiting for its next retry attempt.
- The first event arrives after a long idle period and must retain the existing eager-emission behavior.
- The failure listener or fallback destination itself fails while processing a batching failure.
- Durable file-buffered operation is selected instead of the non-durable in-memory batching path.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The non-durable OpenSearch logging path MUST use the batching capability supplied by the supported Serilog package as the sole owner of its in-memory queue, scheduling, retries, flushing, and batching lifecycle.
- **FR-002**: The published sink package MUST NOT depend directly or transitively on `Serilog.Sinks.PeriodicBatching` through this project.
- **FR-003**: Existing batch posting limit, buffering period, eager first-event behavior, and queue-size configuration MUST map to equivalent native batching behavior.
- **FR-004**: The existing special queue-size value that represents an unbounded queue MUST continue to produce an unbounded queue after migration.
- **FR-005**: The sink MUST expose a configurable maximum retry duration for failed non-durable batches, named `RetryTimeLimit`, through the same options-based configuration surface as existing batching settings.
- **FR-006**: When `RetryTimeLimit` is not specified, the sink MUST use the supported Serilog version's ten-minute native default.
- **FR-007**: Negative retry durations MUST be rejected during logger construction; a zero duration MUST remain valid and disable an extended retry period.
- **FR-008**: Every transport, connection, serialization, protocol, request, or other failure that prevents a valid whole-batch completion MUST propagate from the batch delivery operation regardless of the legacy `ThrowException` failure-handling flag and MUST remain eligible for native retry until `RetryTimeLimit` is exhausted.
- **FR-009**: An unsuccessful batch response with an associated original exception MUST be treated as a failed batch attempt and MUST propagate that failure to the native batching owner.
- **FR-010**: An unsuccessful batch response without an associated exception MUST still produce a failed batch attempt that the native batching owner can diagnose and retry.
- **FR-011**: The sink MUST NOT run a competing retry loop or consume a whole-batch failure after recording it; retry timing, backoff, retry exhaustion, and queue draining MUST remain owned by native batching.
- **FR-012**: Native batching failures MUST remain observable through Serilog's configured failure listener and fallback-chain facilities, including temporary attempt failures, permanent event losses after retry exhaustion, and final losses during shutdown.
- **FR-013**: The sink MUST use native queue-capacity behavior without adding a second sink-owned queue; it MUST NOT intercept or suppress any queue-capacity failure notification that a supported Serilog version emits, and when that version silently discards queue overflow, the sink documentation MUST state that limitation rather than claim fallback notification is available.
- **FR-014**: The sink's legacy per-event failure sink and failure callback MUST apply only to individual bulk-item rejections within a valid completed response; they MUST NOT be invoked for whole-batch failures, which are handled exclusively by native failure listeners and fallback chains.
- **FR-015**: OpenSearch bulk item rejections within an otherwise completed request MUST retain the existing per-event failure-handling behavior and MUST NOT cause already accepted items to be resent as a failed whole batch.
- **FR-016**: A successful retry MUST clear the failed state for that batch and allow subsequent batches to resume the normal configured buffering cadence.
- **FR-017**: Disposal MUST attempt to flush events accepted before disposal and MUST report permanent or final failures through the standard Serilog failure path when the remaining events cannot be delivered.
- **FR-018**: The documented `WriteTo.OpenSearch` registration flow, level restriction, level switch, non-batching OpenSearch options, public `OpenSearchSink` type, and its public constructor MUST remain source-compatible for consumers; only assignability through the removed periodic-batching base class may break.
- **FR-019**: Durable file-buffered operation MUST retain its existing shipping, durability, and failure-handling behavior and MUST NOT be moved onto the non-durable in-memory retry lifecycle by this feature.
- **FR-020**: Package metadata and contributor documentation MUST identify Serilog as the owner of non-durable batching and MUST remove compatibility-package installation or usage guidance.
- **FR-021**: Automated unit coverage MUST verify configuration mapping, retry propagation, recovery, retry exhaustion, failure-listener/fallback integration, partial bulk failures, queue modes, and disposal outcomes without requiring a live OpenSearch cluster.
- **FR-022**: The integration-test project MUST remain unchanged during this feature; live-cluster validation requires separate and specific processing.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A dependency inspection of the packed sink reports zero references to `Serilog.Sinks.PeriodicBatching`.
- **SC-002**: In automated recovery scenarios, 100% of batches that fail and then recover within the configured retry window are attempted again and delivered without resending items already confirmed as accepted.
- **SC-003**: In automated exhaustion scenarios, every event abandoned after the retry window is identifiable in a standard permanent-failure notification or fallback delivery.
- **SC-004**: Automated listener scenarios observe the expected temporary, permanent, and final failure categories for batch-attempt, retry-exhaustion, and shutdown failures respectively.
- **SC-005**: Existing batch size, buffering period, queue-size, unbounded-queue, eager first-event, level restriction, and level-switch behavior pass regression coverage unchanged.
- **SC-006**: Configured retry durations of zero, a representative positive value, and the unspecified default each produce their documented retry behavior; negative values are rejected before events are accepted.
- **SC-007**: Disposing the logger after accepting queued events either delivers all accepted events or reports every abandoned event through the applicable standard failure path.
- **SC-008**: Existing documented non-durable and durable `WriteTo.OpenSearch` consumer configurations compile and retain their intended routing behavior after migration.
- **SC-009**: The production source, built package, documentation, and non-integration test project contain zero active usages of the periodic-batching compatibility API.
- **SC-010**: The integration-test project has no feature-driven source, project, configuration, or execution change.

## Assumptions

- The feature targets the repository's supported Serilog 4.x line, whose native batching default retry duration is ten minutes.
- All failures that prevent a valid whole-batch completion are retryable until native batching exhausts `RetryTimeLimit`; the sink does not apply a competing status- or exception-based retry classifier. Individual bulk item rejections within a valid completed response remain terminal per-event outcomes under the existing failure policy.
- Native Serilog batching is the exclusive owner of failure classification and fallback routing for whole-batch delivery. Legacy sink-specific failure sinks and callbacks remain available only for individual bulk-item rejections within a valid completed response and are never invoked for whole-batch failures.
- In the currently referenced Serilog 4.4 implementation, a bounded queue discards events that cannot be enqueued but does not emit a distinct queue-overflow failure-listener notification. This feature accepts and documents native queue semantics; it will neither defer the migration nor add a second sink-owned queue to synthesize an overflow signal. Adding a new Serilog-level queue-overflow signal is outside this repository's control.
- Compatibility is required for the documented `WriteTo.OpenSearch` configuration surface and direct construction through the existing public `OpenSearchSink` type and constructor. Assignability through the former compatibility base class is the only accepted source-compatibility break.
- Durable file buffering uses a separate lifecycle and is outside the native non-durable batching migration except for regression protection.
- Live OpenSearch integration coverage and migration of `test/Serilog.Sinks.OpenSearch.IntegrationTests` are deferred for separate and specific processing; this feature uses deterministic unit and contract tests.
- The placeholder project constitution contains no ratified constraints beyond the requirements captured in this specification.
