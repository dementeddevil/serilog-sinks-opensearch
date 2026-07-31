# Validation Evidence

## Baseline (2026-07-31)

- Branch: `dev`; SDK: `10.0.302`.
- Scoped production restore/build: PASS, 0 warnings and 0 errors.
- Unit/sample restore/build: existing MSBuild nested `GetTargetFrameworks` failure with exit 1 and no emitted warning/error; combined baseline command timed out before test execution. No integration project command was run.
- Expected pre-feature unit baseline from the plan: 64 cases per target, including five intentional skips per target; not independently confirmed because of the MSBuild failure above.

## Implementation checks

- Production Release build after native migration: PASS, 0 warnings and 0 errors.
- Unit project compile/test remains blocked by the same nested project `GetTargetFrameworks` MSBuild failure with 0 warnings and 0 errors, confirmed with diagnostic verbosity.
- Serial MSBuild (`-m:1`) avoids that environment issue: unit Release builds PASS for net8.0/net9.0/net10.0; the only warnings are the pre-existing obsolete serialization override on net9.0/net10.0.
- Focused `NativeBatching*Tests`: 16/16 PASS independently on net8.0, net9.0, and net10.0 via the generated TUnit executables, including a gated retry of the identical batch followed by recovery, permanent listener notification, fallback routing, partial-item handling, and disposal flushing. `dotnet test` itself incorrectly reports zero discovered tests in this Microsoft Testing Platform configuration, while the executable lists and runs them.
- Complete suite round 1: net8.0 75 total / 70 passed / 5 skipped; net9.0 75 / 70 / 5; net10.0 75 / 70 / 5.
- Sample Release build: PASS; one pre-existing obsolete `FileSink` warning.
- Package: `Serilog.Sinks.OpenSearch.1.5.0-beta1.nupkg` created. Its `.nuspec` contains Serilog 4.4.0 and no `Serilog.Sinks.PeriodicBatching`; `dotnet nuget why` reports no dependency.
- Compatibility audit: after removing stale production XML comments, the only remaining name is the intentional historical removal entry in `CHANGES.md`.
- Whitespace audit: PASS.
- Protected boundary: the scoped integration/solution status changed after baseline capture (additional user-owned deletions/untracked replacements appeared). No feature patch or command targeted those files, but byte-for-byte equality cannot be claimed; integration validation remains deferred.
