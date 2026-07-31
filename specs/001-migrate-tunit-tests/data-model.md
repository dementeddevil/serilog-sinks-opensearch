# Data Model: Unit-Test Migration Evidence

This feature changes unit-test infrastructure rather than production data. The model defines the records needed to prove a complete, correctly bounded migration.

## Unit Test Project

Represents the one independently buildable and executable test assembly in scope.

| Field | Description | Validation |
|---|---|---|
| Name | Stable unit-project identity | Equals `Serilog.Sinks.Opensearch.Tests` |
| ProjectPath | Repository-relative project file | Equals the existing unit-test project path |
| TargetFrameworks | Declared runtime targets | Post-migration set equals the approved pre-migration set |
| ResultArtifact | TRX output ownership | Unique or safely distinguishable per target framework |
| CoverageArtifact | Cobertura output ownership | Produced for applicable unit runs |
| LegacyReferenceCount | Active xUnit references owned by the project | Equals zero at completion |

**Relationships**: Owns many Unit Test Case Baselines and may use multiple Unit Fixture Lifecycles.

## Unit Test Case Baseline

Represents one logical unit test and its expanded executable cases.

| Field | Description | Validation |
|---|---|---|
| StableIdentity | Namespace, class, and method identity | Maps one pre-migration logical test to one migrated logical test |
| DataCaseIdentity | Argument row or source record | Every active pre-migration row has one attributable migrated case |
| TargetFramework | Framework on which the case is discovered | Compared only within the same declared unit target |
| ExpectedStatus | Pass, fail, or skipped | Difference requires documented approval |
| SkipReason | Reason for intentional skip | Required when status is skipped and preserved in meaning |
| MigrationDisposition | Migrated or approved exception | Exceptions require reason and approval reference |

**Relationships**: Belongs to the Unit Test Project and may depend on a Unit Fixture Lifecycle.

## Unit Fixture Lifecycle

Represents setup, shared state, isolation, and cleanup behavior within the unit suite.

| Field | Description | Validation |
|---|---|---|
| Scope | Per-test, per-class, or shared unit state | Must match the behavior required by existing unit tests |
| SetupAction | State created before dependent assertions | Completes before the dependent test executes |
| IsolationKey | Serialization group for mutable state | Required only when overlapping execution is unsafe |
| CleanupAction | State/resource disposal | Attempted after pass, failure, skip, and cancellation |
| AsyncBehavior | Awaited setup and cleanup operations | Failures attributed to the correct lifecycle stage |
| Owner | Test or fixture responsible for disposal | Exactly one clear owner for every created unit resource |

**State transitions**:

```text
Declared -> Starting -> Ready -> InUse -> Disposing -> Disposed
                 |                    |          |
                 +-> Failed ----------+----------+
                                      +-> CleanupFailed
```

## Deferred Scope Boundary

Represents the integration-test project that must remain untouched.

| Field | Description | Validation |
|---|---|---|
| ProjectPath | Deferred directory | `test/Serilog.Sinks.OpenSearch.IntegrationTests` |
| DeferredReason | Why it is excluded | Requires separate and specific processing |
| PermittedChanges | Feature-driven changes allowed | None |
| LegacyExceptions | xUnit matches ignored by scoped audit | Only references owned exclusively by this directory/project |
| BaselineComparison | Source/project/configuration state | Matches the captured pre-implementation state at completion |
| FutureDecisionStatus | Integration migration design | Unspecified until a separate feature |

**Relationship**: Has no execution or lifecycle relationship to this feature's Unit Test Project.

## Unit Test Run Evidence

Represents one validation execution used to close the unit migration.

| Field | Description | Validation |
|---|---|---|
| RunId | Unique execution identity | Required for artifact correlation |
| Framework | Unit target framework executed | Must belong to the Unit Test Project |
| DiscoveryCounts | Logical, expanded, and skipped counts | Must reconcile with baseline or approved exceptions |
| OutcomeCounts | Passed, failed, and skipped | Must reconcile with approved baseline |
| ResultPath | TRX artifact | Exists after the completed applicable run |
| CoveragePath | Cobertura artifact | Exists after the coverage run |
| SequenceNumber | Consecutive stability run number | Three stable complete runs required |

**State transitions**:

```text
Planned -> Discovering -> Running -> CollectingArtifacts -> Validated
              |             |                 |
              +-> Failed ---+-----------------+
                            +-> Cancelled -> CleaningUp
```

## Cross-Entity Validation Rules

1. Every active pre-migration unit-test data row maps to exactly one discoverable migrated case per unit target framework.
2. Every skipped unit baseline remains discoverable with a meaningful reason.
3. Every created unit-test resource has one owning lifecycle and a cleanup outcome.
4. Unit-project and unit-workflow xUnit references equal zero at completion.
5. Integration-project xUnit references are permitted only as explicit deferred-scope exceptions.
6. The Deferred Scope Boundary matches its captured pre-implementation state; pre-existing user changes remain preserved.
7. A complete unit migration has zero unapproved baseline differences and three consistent complete-run evidence sets.
