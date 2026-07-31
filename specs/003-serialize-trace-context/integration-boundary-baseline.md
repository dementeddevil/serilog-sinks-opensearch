# Protected Integration Boundary Baseline

Captured 2026-07-31 without restoring, building, testing, or modifying the solution or integration-test project.

- Scoped `git status --short -- serilog-sinks-opensearch.sln test/Serilog.Sinks.OpenSearch.IntegrationTests` produced no entries.
- Scoped binary-diff hash: `e69de29bb2d1d6434b8b29ae775ad8c2e48c5391` (empty diff).
- `serilog-sinks-opensearch.sln` SHA-256: `22FD3D7DEA0404DCFC896C2A99BA4A17731553513438C380A5350BE00EE13EC8`.
- Integration source hashes:
  - `Bootstrap/OpenSearchFixture.cs`: `0F3A23662DB02C2ADB87CEF02D131D588BF6F7E98136B66565AC311DB42C68D0`
  - `Bootstrap/OpenSearchSinkOptionsFactory.cs`: `D221A0F945A12444B5D2E7EB596001911EC39C6F3480A51711955535AF440F67`
  - `OpenSearchIntegrationScenarios.cs`: `0FA1232DF2F4D56C2F6FC358F461EFDD18C08D91879B892688CBA3939115368F`
  - `OpenSearchVersionTests.cs`: `F5CF2390C4A7C2C5BB21F9CBE9CDEFA44FA5B2CDB14B584A8C3ECBA6570DC529`
  - `README.md`: `4353185DE4478AEDE99F5CC857651D8590F4A80856E0F02EE7A0A35B32FE91E5`
  - `Serilog.Sinks.OpenSearch.IntegrationTests.csproj`: `21E5D555B59AA89915A12D8588BEE2329462DE42DF925700EED24F56FDA53394`

These paths remain excluded from this feature and will be compared after implementation.
