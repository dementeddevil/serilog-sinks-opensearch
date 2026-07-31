# Data Model: Trace and Span Context Formatting

This feature adds no persistent storage. It defines conversion from immutable event metadata and properties to a document.

## Log Event

| Field | Cardinality | Rule | Output |
|---|---:|---|---|
| Trace identifier | Optional | Nullable, non-default 16-byte ID | Root `TraceId` when enabled |
| Span identifier | Optional | Nullable, non-default 8-byte ID | Root `SpanId` when enabled |
| Application properties | Zero or more | Immutable named values | Existing inline/nested output, except conditional collisions |
| Existing metadata | Existing | Existing formatter rules | Unchanged |

The formatter reads only this event instance and retains no context between calls.

## Trace Context

| Attribute | Trace | Span |
|---|---|---|
| Canonical output | 32 lowercase hexadecimal characters | 16 lowercase hexadecimal characters |
| Field | `TraceId` | `SpanId` |
| Default | Enabled | Enabled |
| Placement | Root | Root |

Availability and enablement are independent.

## Formatter Configuration

| Setting | Default | Meaning |
|---|---:|---|
| Render trace identifier | `true` | Emit available trace and make it authoritative |
| Render span identifier | `true` | Emit available span and make it authoritative |
| Inline fields | Existing | Controls ordinary properties only |
| Alternate formatter | None | Owns complete output; built-in trace settings do not apply |

## Collision Matrix

| First-class value | Setting | Matching property | First-class field | Property |
|---|---:|---:|---:|---:|
| Available | Enabled | No | Emitted | N/A |
| Available | Enabled | Yes | Emitted | Suppressed |
| Available | Disabled | Yes/No | Omitted | Preserved if present |
| Absent/default | Enabled | Yes/No | Omitted | Preserved if present |

Matching is ordinal case-insensitive and applies independently to trace/span in inline/nested modes. Filtering never changes source properties used by message rendering.

## State Transition

```text
Receive event
  → evaluate enabled/available IDs
  → write existing root metadata and available IDs
  → scan relevant property collisions
      ├─ none: serialize original properties
      └─ collision: serialize filtered view
  → render metadata from original properties
  → close using existing object/fragment rules
```

Alternate custom/compact formatters bypass this built-in transition and receive the unchanged event.
