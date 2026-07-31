# Research: Serialize Trace and Span Context

## Decision 1: Read immutable event context

**Decision**: Read nullable `LogEvent.TraceId` and `LogEvent.SpanId`; never consult `Activity.Current` while formatting.

**Rationale**: Serilog 4.4 captures IDs when creating the event. Local metadata confirms both properties are nullable read-only activity ID structs; a logger experiment retained exact IDs after the activity ended and returned null outside it. Event-owned data is safe during asynchronous batching and concurrency.

**Alternatives considered**: Ambient lookup was rejected for timing/leak risk; enrichers were rejected as duplicate configuration.

## Decision 2: Use canonical root fields

**Decision**: Emit enabled non-default IDs at the root as `TraceId` (32 lowercase hex) and `SpanId` (16 lowercase hex), using the ID values' canonical string representation. Omit null/default values.

**Rationale**: Serilog's first-party JSON formatter uses these exact names; fixed root placement is stable across property layouts.

**Alternatives considered**: Nested `fields` placement varies by layout; null/zero output misrepresents absence; compact names `@tr`/`@sp` belong to the compact formatter.

## Decision 3: Add independent default-on controls

**Decision**: Append optional `renderTraceId = true` and `renderSpanId = true` controls to built-in formatter constructors and add default-true `RenderTraceId`/`RenderSpanId` sink options plus appended named-configuration parameters. Propagate them to normal and durable built-in factories.

**Rationale**: Direct formatter and default-sink consumers both need independent opt-outs; immutable constructor/options configuration preserves thread safety and existing source calls.

**Alternatives considered**: A combined switch fails independent control; mutable formatter properties weaken concurrency; sink-only settings omit the standalone formatter package.

## Decision 4: Conditional case-insensitive collision suppression

**Decision**: When a first-class ID is emitted, suppress application properties matching its field name with ordinal case-insensitive comparison. When the ID is absent or disabled, preserve the property. Reuse the original dictionary unless a collision requires a filtered view.

**Rationale**: First-class capture-time context stays authoritative without breaking enrichment fallbacks or imposing normal-path allocation.

**Alternatives considered**: Application-property precedence can corrupt correlation; exact-case matching leaves semantic duplicates; unconditional removal loses legitimate untraced enrichment; event mutation violates immutability.

## Decision 5: Preserve original rendering inputs

**Decision**: Filter only document-property serialization. Rendered messages and rendering metadata continue using the original properties.

**Rationale**: Collision handling is a JSON-key rule, not a change to event meaning.

**Alternatives considered**: Replacement events or filtered renderings change established output and alternate-formatter identity.

## Decision 6: Respect formatter ownership

**Decision**: Implement only in the built-in hierarchy. Custom and compact formatters receive the original event and own every output byte; no sink post-processing is added.

**Rationale**: Compact 3.0 already emits its own `@tr`/`@sp` convention, and arbitrary custom output cannot be safely rewritten.

**Alternatives considered**: Wrapping or post-processing alternate formatters risks duplicate fields, invalid non-JSON output, and contract violation.

## Decision 7: Ship at formatter major version 3

**Decision**: Move `Serilog.Formatting.OpenSearch` 2.0.0 to 3.0.0 and coordinate the sink's adoption with its next major release because the sink-created default document changes too. Source constructor/configuration calls remain valid through optional parameters; binary compatibility with already-compiled callers is not promised. No unrelated sink behavior is included.

**Rationale**: Default root fields change indexed schemas for both direct formatter and sink consumers, while constructor parameter additions change CLR signatures. Coordinated major boundaries give consumers a mapping-review/rebuild signal.

**Alternatives considered**: A 2.x default change is too disruptive; default-disabled indefinitely fails the feature's value.

## Decision 8: Separate correctness from performance evidence

**Decision**: Gate correctness and 10,000-event concurrency in TUnit. Verify the 5% throughput criterion through a controlled Release benchmark with prebuilt events, warm-up, interleaved trials, multiple samples, median comparison, and recorded raw evidence.

**Rationale**: A strict wall-clock unit assertion is flaky under shared load; prebuilt events isolate formatter overhead.

**Alternatives considered**: Ordinary timing tests are unstable; skipping measurement would leave SC-008 unverified; a new benchmark dependency is unnecessary initially.

## Decision 9: Exclude live integration

**Decision**: Prove document shape with deterministic formatting/request-capture tests and package inspection; do not touch or invoke the integration project or solution.

**Rationale**: No live cluster is needed to validate serialization, and the integration project is separate user-owned work.

**Alternatives considered**: Live mapping tests add infrastructure risk without improving the formatter contract.
