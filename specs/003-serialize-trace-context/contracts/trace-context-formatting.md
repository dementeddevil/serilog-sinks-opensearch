# Contract: Trace Context Formatting and Compatibility

## Built-in Output

| Condition | Required output |
|---|---|
| Trace enabled and non-default | One root string `TraceId`, exactly 32 lowercase hexadecimal characters |
| Span enabled and non-default | One root string `SpanId`, exactly 16 lowercase hexadecimal characters |
| Identifier disabled, null, or default | No root field for that identifier |

Placement is identical for inline and nested application-property modes.

## Public Configuration

The next formatter major exposes independent default-on trace/span controls through `OpenSearchJsonFormatter`, `ExceptionAsObjectJsonFormatter`, `OpenSearchSinkOptions`, and the sink's named configuration overload. Constructor/configuration parameters are appended and optional so existing source calls retain their meaning. Formatter major v3 communicates that default output and CLR constructor signatures changed; already-compiled consumers rebuild.

Normal and durable built-in formatters receive the settings. A configured custom normal/durable formatter ignores them and exclusively owns output.

## Collision Rules

1. Compare property names ordinally without regard to case.
2. When first-class `TraceId` is emitted, omit all matching application properties.
3. When first-class `SpanId` is emitted, omit all matching application properties.
4. When an ID is disabled/unavailable, preserve similarly named properties normally.
5. Never alter non-conflicting properties, source event properties, rendered messages, or rendering metadata.

A complete built-in document contains at most one semantic trace field and one semantic span field under case-insensitive comparison.

## Ownership and Compatibility

- Derive identifiers only from the `LogEvent` passed to `Format()`; never ambient activity state.
- Do not mutate or replace the event.
- Pass the same event reference to custom/compact formatters and do not rewrite their output.
- Preserve concurrent formatter safety.
- Preserve timestamp, level, message/template, exception, rendering, inline/nested layout, delimiter/fragment behavior, non-conflicting values, and shipping/buffering behavior.

## Release Guidance

The formatter package moves from 2.x to 3.0.0, and the sink adopts it at its next major release because its default document schema changes too. Consumers must review mappings, templates, dashboards, and existing enriched trace/span fields before upgrading. This feature does not create traces, change compact/custom naming, migrate live mappings, or process the integration project.
