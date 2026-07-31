# Compatibility Baseline

Before migration, public `OpenSearchSink(OpenSearchSinkOptions)` inherited `PeriodicBatchingSink`, and therefore supported `ILogEventSink`, `Emit()`, and disposal. `WriteTo.OpenSearch()` applied `MinimumLogEventLevel` and `LevelSwitch`; durable configuration selected `DurableOpenSearchSink`. The production project directly referenced Serilog 4.4.0, OpenSearch.Net 2.0.0, and Serilog.Sinks.PeriodicBatching 5.0.0. The packed package was expected to expose that direct compatibility dependency.

The accepted compatibility boundary is source compatibility for direct construction, `ILogEventSink`, `Emit()`, disposal, level restriction/switch, and unchanged durable routing. Assignment to the removed compatibility base class is intentionally excluded.
