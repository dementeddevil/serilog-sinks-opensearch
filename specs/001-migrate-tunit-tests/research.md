# Research: Migrate Unit Tests to TUnit

## Decision 1: Use the TUnit meta-package in the unit project

**Decision**: Reference stable `TUnit` 1.63.0 in `test/Serilog.Sinks.OpenSearch.Tests` and remove that project's `Microsoft.NET.Test.Sdk`, xUnit packages/runners, `coverlet.msbuild`, and legacy test logger packages once replacement outputs are proven.

**Rationale**: The TUnit meta-package supplies the framework, Microsoft Testing Platform integration, TRX reporting, and coverage support as one supported workflow. Version 1.63.0 was the current stable package returned by `dotnet package search TUnit --exact-match` on 2026-07-31.

**Alternatives considered**:

- Assemble individual TUnit packages: rejected because it creates avoidable version and capability drift.
- Retain VSTest compatibility packages for the unit project: rejected because the feature requires removal of its legacy execution path and TUnit is designed for Microsoft Testing Platform.
- Adopt prerelease TUnit packages: rejected because no required unit-migration capability depends on prerelease behavior.

**Sources**: [TUnit installation](https://tunit.dev/docs/getting-started/installation/), [running tests](https://tunit.dev/docs/getting-started/running-your-tests/), [Microsoft `dotnet test`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)

## Decision 2: Use TUnit Assertions throughout

**Decision**: Remove FluentAssertions from the unit project and convert every live assertion to the awaited TUnit `Assert.That(...)` API.

**Rationale**: A post-plan maintainer decision requested one test framework and assertion stack. Awaited TUnit assertions remove the additional assertion dependency, are analyzed as part of the TUnit build, and keep assertion execution explicit. Async setup hooks replace constructor-time assertions where awaiting was not possible.

**Supersedes**: The initial plan to retain FluentAssertions.

**Alternatives considered**:

- Convert every assertion to TUnit assertions: rejected because it expands scope and risks behavioral drift without helping discovery or lifecycle migration.
- Keep `xunit.assert` in the unit project: rejected because it violates the scoped zero-xUnit requirement and leaves a misleading dependency.

## Decision 3: Apply explicit, typed TUnit mappings

**Decision**: Use these mappings within the unit project and verify expanded discovery after each conversion:

| xUnit construct | TUnit construct |
|---|---|
| `[Fact]` | `[Test]` |
| `[Theory]` with `[InlineData]` | `[Test]` with `[Arguments]` |
| `[MemberData]` | Typed `[MethodDataSource]` provider |
| `Fact(Skip = reason)` | `[Test]` plus `[Skip(reason)]` |
| Collection/nonparallel grouping | Keyed `[NotInParallel]` isolation |
| `IClassFixture<T>` | Shared `[ClassDataSource<T>]` fixture |
| `IDisposable` test cleanup | Explicit TUnit `[After(Test)]` lifecycle where test-scoped cleanup is intended |

**Rationale**: TUnit is parallel by default and its data-source and lifecycle APIs differ from xUnit. Explicit mappings preserve case identity, lifetime, skip visibility, and isolation instead of relying on superficial attribute renames.

**Alternatives considered**:

- Mechanical attribute renaming only: rejected because member data, shared fixtures, cleanup, and concurrency have different semantics.
- Globally disable parallelism: rejected because it hides missing isolation declarations and needlessly slows independent unit tests.

**Sources**: [xUnit migration guide](https://tunit.dev/docs/migration/xunit/), [class data sources](https://tunit.dev/docs/writing-tests/class-data-source/), [parallelism](https://tunit.dev/docs/execution/parallelism/)

## Decision 4: Standardize the unit workflow on Microsoft Testing Platform

**Decision**: Add a .NET 10 `global.json` test-runner selection for `Microsoft.Testing.Platform`; use `dotnet test` with TUnit tree-node filters for focused unit runs, `--report-trx` for machine-readable results, and `--coverage --coverage-output-format cobertura` for coverage. CI runs the unit project explicitly and feeds its TRX files to the existing test reporter.

**Rationale**: This is TUnit's native supported runner path and preserves the repository's essential unit result/coverage contracts without retaining VSTest-only HTML or GitHubActions logger packages for the migrated project.

**Alternatives considered**:

- Keep the existing `--logger html --logger trx --logger GitHubActions` unit command: rejected because those logger paths are tied to legacy VSTest packages.
- Depend only on solution-wide implicit discovery: rejected because an explicit unit-project command makes the scope and artifact ownership clear while integration remains deferred.

**Sources**: [TUnit filters](https://tunit.dev/docs/execution/test-filters/), [TUnit coverage](https://tunit.dev/docs/extending/code-coverage/), [running tests](https://tunit.dev/docs/getting-started/running-your-tests/)

## Decision 5: Treat the integration project as a protected deferred boundary

**Decision**: Make no change to `test/Serilog.Sinks.OpenSearch.IntegrationTests`, its xUnit packages, sources, bootstraps, fixtures, cluster configuration, target framework, current solution membership, or execution behavior. Exclude its references from the unit migration's zero-xUnit audit and require a later specification and plan before processing it.

**Rationale**: The user explicitly deferred this project because its external-service and lifecycle concerns require separate, specific processing. A partial conversion or speculative infrastructure choice would make its state harder to understand and could weaken future compatibility validation.

**Alternatives considered**:

- Convert simple integration attributes now and defer fixtures: rejected because it would leave a mixed, potentially undiscoverable project.
- Replace the integration cluster harness in this feature: rejected because it expands the newly narrowed scope and pre-decides future lifecycle and compatibility work.
- Remove integration xUnit references to satisfy a repository-wide audit: rejected because those references remain necessary to unchanged deferred code.

## Decision 6: Use a unit-only executable baseline

**Decision**: Capture discovery and outcome evidence only for `test/Serilog.Sinks.OpenSearch.Tests`, per target framework. Separately capture a scoped integration-project diff baseline solely to prove no feature-driven edits occurred; do not claim that integration tests were executed or migrated.

**Rationale**: This yields measurable parity for the intended change while keeping deferred project health outside the completion decision. It also prevents repository-wide xUnit matches from being misreported as incomplete unit migration work.

**Alternatives considered**:

- Require integration discovery as part of the baseline: rejected because the project is explicitly outside scope.
- Ignore the integration directory entirely: rejected because implementation needs a simple guard proving that excluded files stayed untouched.
