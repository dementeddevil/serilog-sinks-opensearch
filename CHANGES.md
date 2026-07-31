# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added default-on first-class Serilog `TraceId` and `SpanId` output in `Serilog.Formatting.OpenSearch` 3.1.0.
- Added independent `RenderTraceId` and `RenderSpanId` formatter and sink opt-outs.
- Added nullable `RetryTimeLimit` configuration and standard temporary, permanent, and final failure reporting through Serilog failure listeners and fallback chains.
- Added Testcontainers integration coverage for OpenSearch 1.3.20 and 2.19.6, including indexed trace-context verification.

### Changed

- Updated the sink to `OpenSearch.Net` 2.0.0 and Serilog 4.4.0.
- Migrated unit and integration tests from xUnit to TUnit with Microsoft Testing Platform; unit coverage now targets .NET 8, .NET 9, and .NET 10.
- Replaced compatibility batching with Serilog 4.4 native batching for non-durable sinks.
- Whole-batch transport failures now propagate for retry and fallback handling; legacy failure handlers remain scoped to rejected items in otherwise valid bulk responses.
- First-class trace/span identifiers take precedence over case-insensitive application-property collisions, while custom and compact formatters retain exclusive output ownership.
- Preserved direct-construction source compatibility and the existing durable file-buffering lifecycle.

### Removed

- Removed the `Serilog.Sinks.PeriodicBatching` package dependency.
