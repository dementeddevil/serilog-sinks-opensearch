# Compatibility Baseline

Captured 2026-07-31 from scoped source and packages.

## Public formatter construction

- `DefaultJsonFormatter` protected constructor: enclosing-object, delimiter, rendered-message, format-provider, and rendered-template controls.
- `OpenSearchJsonFormatter` public constructor additionally accepts serializer, inline fields, stack-trace array, and TimeSpan-as-ticks controls.
- `ExceptionAsObjectJsonFormatter` publicly forwards all of those except TimeSpan-as-ticks.
- None has a trace/span control before this feature.

## Sink defaults and ownership

- `OpenSearchSinkOptions.CustomFormatter` and `CustomDurableFormatter` are optional.
- `OpenSearchSinkState` selects each custom formatter unchanged when supplied.
- Otherwise its normal and durable factories create `OpenSearchJsonFormatter`; their delimiter differs (`string.Empty` vs newline).
- Default sink options have no trace/span controls before this feature.

## Package baseline

- Formatter package: `Serilog.Formatting.OpenSearch` 2.0.0, depending on Serilog 4.4.0.
- Sink package: `Serilog.Sinks.OpenSearch` 1.5.0-beta1.
- Packed sink dependency: exact `Serilog.Formatting.OpenSearch` 2.0.0, plus OpenSearch.Net 2.0.0, Serilog 4.4.0, Serilog.Formatting.Compact 3.0.0, Serilog.Sinks.File 7.0.0, and System.Diagnostics.DiagnosticSource 10.0.10.
- Baseline packages were created under ignored `artifacts/trace-context-baseline/` using only the formatter and sink project files.
