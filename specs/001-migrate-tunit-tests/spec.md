# Feature Specification: Migrate Unit Tests to TUnit

**Feature Branch**: `dev`

**Created**: 2026-07-31

**Updated**: 2026-07-31

**Status**: Draft

**Input**: User description: "switch from xunit to tunit and migrate all tests", subsequently narrowed to exclude the integration-test project until it receives separate and specific processing.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run the Complete Migrated Unit Suite (Priority: P1)

As a maintainer, I can run every active automated test in the unit-test project through TUnit and receive the same verification coverage and outcomes that the current xUnit unit suite provides, so dependency upgrades and product changes remain safe.

**Why this priority**: Preserving the in-scope regression safety net is the primary purpose of the migration. The framework switch is not successful if unit tests disappear, stop being discovered, or change meaning.

**Independent Test**: Capture the pre-migration inventory of active and intentionally skipped unit-test cases, run the migrated unit suite, and compare discovery, execution, and outcomes across every target framework declared by that project.

**Acceptance Scenarios**:

1. **Given** the pre-migration inventory of active unit tests, **When** the migrated unit-test suite is discovered, **Then** every active test case has a corresponding discoverable test with equivalent assertions and input cases.
2. **Given** the pre-migration inventory of intentionally skipped unit tests, **When** the migrated suite is discovered, **Then** each remains explicitly skipped with its reason preserved.
3. **Given** a target framework declared by the unit-test project, **When** the complete migrated unit suite runs, **Then** its pass, fail, and skip outcomes match the approved baseline except for differences documented and approved as migration corrections.

---

### User Story 2 - Use One Supported Unit-Test Workflow (Priority: P2)

As a contributor, I can build, run, filter, debug, and collect coverage for the migrated unit tests locally and in continuous integration without installing or invoking legacy xUnit-specific tooling for that project.

**Why this priority**: A single supported unit-test workflow reduces maintenance overhead and prevents contributors or automation from accidentally using an obsolete runner for the migrated project.

**Independent Test**: Follow the documented local and continuous-integration unit-test workflows from a clean checkout and confirm that discovery, filtering, reporting, and coverage artifacts work without an xUnit dependency in the unit-test project.

**Acceptance Scenarios**:

1. **Given** a clean checkout with documented prerequisites, **When** a contributor follows the standard unit-test command, **Then** all target frameworks declared by the unit-test project are discovered and executed.
2. **Given** a contributor needs a focused run, **When** unit tests are filtered by supported identifiers, **Then** only the selected tests run and their results remain clearly attributable.
3. **Given** a continuous-integration run, **When** unit tests complete, **Then** machine-readable results and coverage artifacts are published in the formats expected by the repository workflow.
4. **Given** the unit migration is complete, **When** the unit project and its execution paths are inspected, **Then** no active xUnit dependency or xUnit-specific unit-test command remains.

---

### User Story 3 - Defer Integration Migration Safely (Priority: P3)

As a maintainer, I can complete and review the unit-test migration without altering the integration-test project, so its framework-specific cluster lifecycle can be handled later as a separate, purpose-built change.

**Why this priority**: The integration suite has distinct external-service, lifecycle, dependency, and isolation concerns. Separating it prevents the unit migration from silently changing or partially replacing that behavior.

**Independent Test**: Compare the integration-test project and its project-specific infrastructure before and after this feature and confirm that it is unchanged and explicitly excluded from unit-migration completion checks.

**Acceptance Scenarios**:

1. **Given** the current integration-test project, **When** this feature is implemented, **Then** its project file, test sources, bootstraps, fixtures, cluster configuration, and execution behavior remain unchanged.
2. **Given** repository-wide xUnit references that belong exclusively to the deferred integration project, **When** the unit migration is audited, **Then** those references are reported as an explicit scoped exception rather than removed or counted as a unit-migration failure.
3. **Given** future work on the integration suite, **When** that work is planned, **Then** its cluster provisioning, readiness, isolation, target compatibility, and cleanup requirements are specified independently of this feature.

### Edge Cases

- Parameterized unit tests with multiple inline or member-supplied data rows must retain every distinct case and input value.
- Unit tests that share fixtures or static state must preserve the intended setup frequency and resource ownership.
- Unit tests marked for non-parallel or isolated execution must not become concurrent because of different framework defaults.
- Asynchronous unit-test setup, execution, and cleanup failures must be reported against the correct test or lifecycle stage.
- Intentionally skipped flaky unit tests must remain visible and skipped with their existing reason rather than silently disappearing or becoming enabled.
- Unit tests targeting multiple runtime versions must not be counted as missing merely because the same logical case runs once per target.
- Existing commented-out unit-test code is not considered an active test, but it must not be accidentally re-enabled or deleted solely by this migration.
- Repository-wide searches will continue to find xUnit references inside the excluded integration-test project; audits must distinguish those scoped exceptions from references owned by the migrated unit workflow.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The migration MUST include every automated test in `test/Serilog.Sinks.OpenSearch.Tests`, including formatter, durable-shipping, template, and other unit-level coverage within that project.
- **FR-002**: The team MUST record a pre-migration inventory of discoverable unit-test cases, data-driven cases, skipped tests, target frameworks, and expected outcomes for comparison after migration.
- **FR-003**: Every active xUnit unit test in scope MUST have a discoverable TUnit equivalent that preserves its intent, assertions, inputs, and expected outcome.
- **FR-004**: Data-driven unit tests MUST preserve every existing input row and make failures attributable to the failing input.
- **FR-005**: Intentionally skipped unit tests MUST remain explicitly represented, retain their skip reason, and remain excluded from normal execution unless separately approved for re-enablement.
- **FR-006**: Unit-test setup, shared fixture, per-test fixture, and cleanup behavior MUST preserve the existing lifecycle scope and ordering required by each test group.
- **FR-007**: Unit tests and groups that require isolation or serialized execution MUST retain that protection after migration.
- **FR-008**: `test/Serilog.Sinks.OpenSearch.IntegrationTests` and all project-specific source, configuration, dependency, lifecycle, and execution behavior beneath it MUST be excluded from this feature and left unchanged.
- **FR-009**: Active xUnit references owned exclusively by the excluded integration-test project MUST be documented as a scoped exception and MUST NOT be removed or modernized by this feature.
- **FR-010**: The migrated unit suite MUST continue to run against every target framework declared by the unit-test project unless a target change is separately documented and approved.
- **FR-011**: Local workflows MUST support complete unit-suite execution, individual target execution where supported, focused unit-test selection, debugging, result reporting, and coverage collection.
- **FR-012**: Continuous-integration workflows MUST execute the migrated unit project and continue producing the test-result and coverage artifacts consumed by the repository's reporting process.
- **FR-013**: All active xUnit package references, runner references, source imports, attributes, configuration, tool references, suppressions, and automation commands owned by the unit-test project or its unit-only execution path MUST be removed or replaced.
- **FR-014**: The production libraries and their public behavior MUST remain unchanged by the unit-test framework migration.
- **FR-015**: Any intentional difference from the pre-migration unit-test inventory or outcome baseline MUST be documented with its reason and approved before the migration is considered complete.
- **FR-016**: Contributor documentation MUST describe the migrated local and continuous-integration unit-test workflows and explicitly state that integration-test migration is deferred.
- **FR-017**: The migration MUST preserve reliable cleanup for in-scope unit-test resources when a test passes, fails, is skipped, or is cancelled.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of active pre-migration unit-test cases, including every data-driven case, have an equivalent discoverable post-migration case or an explicitly approved exception.
- **SC-002**: 100% of intentionally skipped unit tests remain discoverable as skipped and retain a meaningful reason.
- **SC-003**: The complete migrated unit suite produces the same approved pass, fail, and skip baseline on every target framework declared by the unit-test project.
- **SC-004**: A scoped inspection finds zero active xUnit dependencies, imports, test attributes, bootstrap declarations, or execution commands owned by the unit-test project or its unit-only workflow.
- **SC-005**: From a clean checkout, a contributor can start the complete unit suite using the documented workflow without installing legacy framework-specific tooling for that project.
- **SC-006**: Continuous integration publishes unit-test results and coverage artifacts for every target framework required by the migrated project.
- **SC-007**: Three consecutive complete unit-suite validation runs produce consistent discovery counts and outcomes, excluding tests already documented as intentionally skipped.
- **SC-008**: No production-library source or public behavior changes are required to complete the unit migration.
- **SC-009**: The excluded integration-test project has no feature-driven source, project, configuration, or execution change.

## Assumptions

- This feature covers only `test/Serilog.Sinks.OpenSearch.Tests`; `test/Serilog.Sinks.OpenSearch.IntegrationTests` requires a later, separate specification and implementation plan.
- Current unit assertions and test intent are authoritative; changing product expectations is outside this feature unless the migration exposes a confirmed defect and the change is separately approved.
- Existing intentionally skipped unit tests remain skipped. Resolving their underlying flakiness is outside this feature.
- Existing commented-out unit tests are not part of the discoverable baseline and will not be reactivated solely as part of the migration.
- The unit project's existing target frameworks and coverage/reporting expectations remain in scope.
- Existing xUnit dependencies and infrastructure inside the excluded integration-test project remain temporarily and do not prevent this unit-only feature from completing.
- The placeholder project constitution contains no ratified constraints beyond the requirements captured in this specification.
