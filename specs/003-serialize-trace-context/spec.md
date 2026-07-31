# Feature Specification: Serialize Trace and Span Context

**Feature Branch**: `dev`

**Created**: 2026-07-31

**Status**: Draft

**Input**: User description: "Serialize Serilog's first-class TraceId and SpanId. Serilog automatically captures Activity.Current.TraceId and SpanId into LogEvent, but the formatter currently discards them unless applications add separate enriched properties. Add optional TraceId and SpanId fields, enabled by default for the new formatter major version, with coverage inside and outside an Activity, inline and nested properties, compact/custom formatter behavior, and collisions with similarly named application properties."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Preserve Distributed Trace Correlation (Priority: P1)

As an operator investigating logs from a traced request, I can see the event's trace and span identifiers in the indexed document without configuring duplicate enrichment, so I can move reliably between logs and distributed traces.

**Why this priority**: Trace correlation is the primary value of the feature. Losing identifiers already captured on an event prevents operators from connecting log records to the operation that produced them.

**Independent Test**: Format one event created while trace context is active and one event created without trace context. The traced document contains its canonical trace and span identifiers, while the untraced document contains neither field.

**Acceptance Scenarios**:

1. **Given** an event captured with a non-empty trace identifier and span identifier, **When** the built-in formatter produces a document, **Then** the document contains `TraceId` and `SpanId` with the event's canonical identifier values.
2. **Given** an event captured without trace context, **When** the built-in formatter produces a document, **Then** neither `TraceId` nor `SpanId` is emitted as an empty, null, or zero-valued field.
3. **Given** consecutive events from different traced operations, **When** they are formatted, **Then** each document contains only the identifiers belonging to its own event.

---

### User Story 2 - Keep Documents Unambiguous Across Property Layouts (Priority: P2)

As a consumer querying OpenSearch documents, I receive one authoritative trace field and one authoritative span field regardless of whether application properties are nested or written inline, avoiding ambiguous mappings and duplicate keys.

**Why this priority**: Duplicate or conflicting identifiers make correlation unreliable and can produce different query behavior between property-layout modes.

**Independent Test**: Format traced events in both nested-property and inline-property modes, including application properties whose names differ from `TraceId` or `SpanId` only by letter case. Each document is valid JSON, contains one authoritative field for each available identifier, and retains all non-conflicting properties.

**Acceptance Scenarios**:

1. **Given** traced events with nested application properties, **When** documents are produced, **Then** the first-class identifiers appear at the document root and ordinary application properties retain their nested layout.
2. **Given** traced events with inline application properties, **When** documents are produced, **Then** the first-class identifiers and ordinary application properties appear at the document root without duplicate keys.
3. **Given** a traced event with an application property named `TraceId`, `SpanId`, or a casing variant, **When** the document is produced, **Then** the first-class event value is emitted once and the conflicting application property is not emitted a second time.
4. **Given** an untraced event with an application property named `TraceId` or `SpanId`, **When** the document is produced, **Then** the application property remains available in its configured property layout because no first-class value exists to supersede it.

---

### User Story 3 - Control Adoption Without Changing Formatter Ownership (Priority: P3)

As a library consumer adopting the next formatter major release, I receive trace fields by default, can suppress either field when my schema requires it, and continue to control the complete output when I select another formatter.

**Why this priority**: Default-on behavior provides immediate value, while opt-out and formatter isolation let consumers preserve established schemas and specialized output contracts.

**Independent Test**: Verify the next major formatter defaults, each opt-out combination, and explicitly selected compact and custom formatters. Built-in output follows the selected trace settings; alternate formatters receive the unchanged event and remain solely responsible for their output.

**Acceptance Scenarios**:

1. **Given** the next major built-in formatter with no trace-specific configuration, **When** a traced event is formatted, **Then** both trace fields are emitted.
2. **Given** trace or span output is individually disabled, **When** a traced event is formatted, **Then** the disabled field is absent and the other enabled field retains its normal behavior.
3. **Given** both trace fields are disabled, **When** a traced event is formatted, **Then** output matches the established document shape apart from unrelated options.
4. **Given** a consumer explicitly selects a compact or custom formatter, **When** a traced event is sent, **Then** the selected formatter receives the original event and this feature neither injects, removes, renames, nor post-processes fields in its output.

### Edge Cases

- An event has a trace identifier but no span identifier, or vice versa; each available enabled identifier is handled independently.
- An identifier is present as its empty/default value; it is treated as absent rather than serialized.
- Application properties use `TraceId` or `SpanId` with different letter casing.
- A colliding application property has a non-string or structured value.
- Property rendering is configured with or without an enclosing object or with a custom closing delimiter; trace output must not invalidate the document fragment or delimiter behavior.
- An event contains formatted message-template tokens referencing a colliding property; suppressing a duplicate output field must not prevent the event's existing rendered message or rendering metadata from being produced.
- Multiple threads format traced and untraced events concurrently; identifiers must never leak between events.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The built-in event formatter MUST recognize the event's first-class trace identifier independently of application-defined properties.
- **FR-002**: The built-in event formatter MUST recognize the event's first-class span identifier independently of application-defined properties.
- **FR-003**: When enabled and non-empty, the trace identifier MUST be emitted at the document root as `TraceId` using its canonical 32-character hexadecimal representation.
- **FR-004**: When enabled and non-empty, the span identifier MUST be emitted at the document root as `SpanId` using its canonical 16-character hexadecimal representation.
- **FR-005**: Empty, default, or unavailable first-class identifiers MUST be omitted rather than represented by null, an empty string, or all-zero text.
- **FR-006**: Trace and span output MUST be enabled by default in the next major version of the built-in formatter.
- **FR-007**: Consumers MUST be able to disable trace identifier output and span identifier output independently.
- **FR-008**: Existing formatting options for messages, templates, exceptions, delimiters, and enclosing objects MUST continue to compose with trace output.
- **FR-009**: First-class identifiers MUST use the same root-level location in both nested-property and inline-property modes.
- **FR-010**: All non-conflicting application properties MUST retain their existing names, values, and configured nesting behavior.
- **FR-011**: When a non-empty first-class trace identifier is emitted, an application property whose name matches `TraceId` without regard to letter case MUST NOT create another trace identifier field anywhere in the document.
- **FR-012**: When a non-empty first-class span identifier is emitted, an application property whose name matches `SpanId` without regard to letter case MUST NOT create another span identifier field anywhere in the document.
- **FR-013**: If the corresponding first-class identifier is unavailable or disabled, a similarly named application property MUST retain its normal output behavior.
- **FR-014**: First-class event values MUST take precedence over colliding application property values whenever both would otherwise be emitted.
- **FR-015**: Collision handling MUST NOT alter rendered messages, message-template processing, or rendering metadata derived from the original event.
- **FR-016**: Explicitly selected compact and custom formatters MUST retain exclusive ownership of their output; the sink MUST pass the original event to them without trace-field post-processing.
- **FR-017**: The formatter MUST remain safe for concurrent use and MUST derive identifiers only from the event being formatted, never from ambient context at formatting time.
- **FR-018**: Output MUST remain syntactically valid for complete JSON documents and for the formatter's supported unenclosed fragment mode.
- **FR-019**: Existing events without first-class trace context MUST retain their previous built-in document shape except where a consumer explicitly changes trace-related configuration.
- **FR-020**: Public guidance for the new formatter major version MUST state the default field names, canonical value formats, omission behavior, collision precedence, opt-out controls, and the boundary for compact/custom formatters.

### Key Entities

- **Log Event**: An immutable logging record containing timestamp, level, message information, exception, application properties, and optional first-class trace and span identifiers captured when the event was created.
- **Trace Context**: The optional pair of identifiers that correlates an event with a distributed trace and a specific operation within that trace. Either identifier may be absent independently.
- **Application Property**: A named value supplied by the application or an enricher. It retains its configured inline or nested placement unless its name conflicts with an emitted first-class identifier.
- **Formatter Configuration**: The consumer-selected settings controlling trace output, property layout, message rendering, delimiters, and whether a built-in or alternate formatter owns the document.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a representative traced-event test set, 100% of produced built-in documents contain the exact trace and span identifiers captured on their source events.
- **SC-002**: In a representative untraced-event test set, 100% of produced built-in documents omit empty, default, and null trace/span fields.
- **SC-003**: Across inline and nested property layouts, every tested document parses successfully and contains no more than one semantic `TraceId` field and one semantic `SpanId` field when names are compared without regard to case.
- **SC-004**: All combinations of default, trace-disabled, span-disabled, and both-disabled settings produce the documented field set in 100% of acceptance tests.
- **SC-005**: Existing formatter regression tests for messages, templates, exceptions, property layouts, delimiters, and unenclosed fragments continue to pass without changed expected behavior unrelated to trace context.
- **SC-006**: Explicit compact/custom formatter tests demonstrate zero sink-added, sink-removed, or sink-renamed trace fields in their output.
- **SC-007**: Concurrent formatting of at least 10,000 mixed traced and untraced events produces no identifier crossover and no malformed documents.
- **SC-008**: Formatting representative traced events introduces no more than 5% throughput degradation compared with equivalent untraced events under the same conditions.

## Assumptions

- The logging pipeline has already captured trace and span identifiers into each event; this feature does not create activities, start traces, or consult ambient trace context during formatting.
- `TraceId` and `SpanId` are the stable built-in document field names for this formatter major version.
- Canonical identifiers use lowercase hexadecimal text with fixed widths of 32 characters for a trace and 16 characters for a span.
- Name-collision comparison is case-insensitive to prevent semantically duplicate OpenSearch fields such as `TraceId` and `traceid`.
- First-class event context is authoritative when available; application-defined similarly named values are compatibility fallbacks only when the corresponding first-class output is absent or disabled.
- The default-on change is introduced at the next formatter major-version boundary so consumers can review mapping and schema implications before upgrading.
- Compact and consumer-supplied custom formatters are separate output contracts and are not modified by this feature.
- Deterministic formatter coverage remains the primary contract. A user-authorized follow-up also verifies the indexed document shape against the repository's existing OpenSearch 1.x and 2.x Testcontainers fixtures.
