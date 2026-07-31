# Implementation Plan: Serialize Trace and Span Context

**Branch**: `dev` | **Date**: 2026-07-31 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-serialize-trace-context/spec.md`

**Note**: Formatter and unit-contract work was implemented first with the integration project protected. On 2026-07-31 the user explicitly brought the live integration project into scope for follow-up trace-context coverage; the solution itself remains unchanged.

## Summary

Serialize Serilog 4.4's nullable first-class trace and span identifiers as canonical root-level `TraceId` and `SpanId` fields in the built-in OpenSearch formatter hierarchy. Enable both by default at the formatter package's next major-version boundary, expose independent opt-outs through formatter constructors and default sink options, suppress case-insensitive application-property collisions only when the corresponding first-class value is emitted, and leave rendering metadata plus custom/compact formatter output untouched.

The implementation reads identifiers from the immutable `LogEvent`, never `Activity.Current`. `DefaultJsonFormatter` owns event metadata and collision preparation; `OpenSearchJsonFormatter` owns public field names and controls; `ExceptionAsObjectJsonFormatter` forwards settings; and `OpenSearchSinkState` propagates settings only when constructing built-in normal or durable formatters.

## Technical Context

**Language/Version**: C# (`LangVersion=latest`), .NET SDK 10.0.302; production targets .NET Standard 2.0

**Primary Dependencies**: Serilog 4.4.0 (`LogEvent.TraceId`/`SpanId`); existing formatter and sink projects; no new runtime dependency

**Storage**: N/A — immutable events are formatted into JSON text

**Testing**: TUnit 1.63.0 with Microsoft Testing Platform on `net8.0`, `net9.0`, and `net10.0`; parsed-JSON, activity capture, collision, ownership, concurrency, packaging, and controlled benchmark coverage

**Target Platform**: Any .NET Standard 2.0-compatible consumer; repository validation on Windows

**Project Type**: Multi-package .NET library with unit-test executable and sample consumer

**Performance Goals**: No crossover/malformed output across 10,000 concurrent mixed events; traced formatting throughput at least 95% of equivalent untraced throughput in a controlled Release benchmark

**Constraints**: Canonical lowercase 32/16-character values; omit null/default IDs; no ambient lookup; case-insensitive collision precedence; valid complete/fragment JSON; alternate formatter ownership; appended optional public parameters; live integration tests use the existing OpenSearch 1.x/2.x Testcontainers fixtures

**Scale/Scope**: Formatter hierarchy, sink option/factory wiring, named configuration, focused unit contracts, docs/sample/changelog, formatter/sink package inspection, and follow-up live indexed-document coverage on OpenSearch 1.x and 2.x

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

The constitution is an unratified placeholder with no enforceable principles. The validated specification and repository conventions provide the gates:

- **Scope — PASS**: Initial implementation was limited to built-in formatting/configuration/unit validation. The later user-authorized follow-up adds only trace-context scenarios to the existing integration project.
- **Compatibility — PASS**: Existing source calls survive appended optional parameters/defaulted options. Formatter v3 records the intentional output-schema and constructor binary change.
- **Ownership — PASS**: Built-in formatters own built-in output; custom/compact formatters receive the original event without rewriting.
- **Integrity — PASS**: IDs come from each event, are canonical/optional, and cannot leak from ambient state.
- **Testability — PASS**: Output, collisions, configuration, concurrency, packaging, and performance have explicit validation paths.

Post-design re-check: **PASS**. Research resolved all choices; contracts define precedence and compatibility; no persistent-data, security, or live-integration gate is introduced.

## Project Structure

### Documentation (this feature)

```text
specs/003-serialize-trace-context/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/trace-context-formatting.md
└── checklists/requirements.md
```

### Source Code (repository root)

```text
src/
├── Serilog.Formatting.OpenSearch/
│   ├── DefaultJsonFormatter.cs
│   ├── OpenSearchJsonFormatter.cs
│   ├── ExceptionAsObjectJsonFormatter.cs
│   └── Serilog.Formatting.OpenSearch.csproj
└── Serilog.Sinks.OpenSearch/
    ├── Sinks/OpenSearch/OpenSearchSinkOptions.cs
    ├── Sinks/OpenSearch/OpenSearchSinkState.cs
    └── LoggerConfigurationOpenSearchExtensions.cs

test/Serilog.Sinks.OpenSearch.Tests/
├── OpenSearchJsonFormatterTests.cs
├── TraceContextFormattingTests.cs
├── TraceContextCollisionTests.cs
├── TraceContextCompatibilityTests.cs
├── TraceContextConcurrencyTests.cs
└── Serilog.Sinks.Opensearch.Tests.csproj

test/Serilog.Sinks.OpenSearch.IntegrationTests/
├── TraceContextIntegrationScenarios.cs
├── OpenSearchVersionTests.cs
└── README.md

sample/Serilog.Sinks.OpenSearch.Sample/
├── Program.cs
└── appsettings.json
```

**Structure Decision**: Extend the existing hierarchy/option flow and TUnit projects. Do not add an enricher, serializer wrapper, package, benchmark dependency, or another integration project; reuse the live OpenSearch fixtures already present.

## Implementation Strategy

### Event metadata and output

1. Add independently defaulted trace/span settings to the built-in formatter hierarchy.
2. Read nullable IDs from the supplied event, reject default IDs, and write canonical strings through overridable metadata writers.
3. Define public `TraceId`/`SpanId` field-name constants on `OpenSearchJsonFormatter`.
4. Keep both fields at the root in inline and nested layouts.

### Collision and configuration

1. Scan for case-insensitive collisions only for first-class fields actually emitted.
2. Reuse the original dictionary normally; allocate a filtered view only on collision. Avoid an empty `fields` container when collisions remove every document property.
3. Continue message/rendering metadata from the original event properties.
4. Add default-true `RenderTraceId`/`RenderSpanId` sink options and append matching named-configuration parameters; pass them into normal/durable built-in factories only.
5. Preserve `CustomFormatter`/`CustomDurableFormatter` as complete-output boundaries, including compact formatters.

### Compatibility and release

1. Append optional parameters to `OpenSearchJsonFormatter` and `ExceptionAsObjectJsonFormatter`; evolve the protected base surface consistently.
2. Release the default/schema and CLR signature change as `Serilog.Formatting.OpenSearch` 3.0.0 and coordinate the sink's adoption with its next major release because its default indexed document also changes. Existing source calls compile; already-compiled consumers rebuild for the majors.
3. Document mappings, defaults, opt-outs, precedence, and alternate-formatter ownership in README, sample, and changelog.
4. Pack formatter and sink; inspect formatter v3 metadata and the sink's dependency relationship.

## Validation Strategy

- Real logger events inside/outside a W3C activity prove capture timing; explicit fixed-ID events prove exact values/default handling.
- Parse complete JSON; separately assert unenclosed fragments and delimiters.
- Cover inline/nested layout, casing variants, structured collisions, disabled fallbacks, template rendering, and all-properties-collide behavior.
- Recording custom and compact formatter paths prove identical event reference and zero sink post-processing.
- One formatter handles 10,000 prebuilt mixed events concurrently; every document parses and maps to its source event.
- A separate idle-machine Release benchmark uses prebuilt events, warm-up, interleaved trials, multiple samples, and median comparison. It is evidence for SC-008, not a timing-sensitive ordinary unit gate.
- Restore/build/test/pack formatter, sink, unit, sample, and—after the explicit scope expansion—the existing integration project. The solution remains unnecessary.

## Complexity Tracking

No constitution violations or extra architectural layers require justification.
