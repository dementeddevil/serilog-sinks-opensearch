# OpenSearch integration tests

This TUnit project verifies the sink against real OpenSearch nodes managed by
Testcontainers for .NET.

## Version matrix

| OpenSearch line | Docker image | Scenarios |
|---|---|---|
| 1.x | `opensearchproject/opensearch:1.3.20` | Explicit OSv1 template and detected-version template |
| 2.x | `opensearchproject/opensearch:2.19.6` | Explicit OSv1 template and detected-version template |

Each scenario verifies template registration, index-pattern compatibility,
minimum-level filtering, final batch flushing, and indexed log levels. Security
is disabled only inside the ephemeral test containers.

## Run

Docker must be running and able to use Linux containers. The first run can take
several minutes while Docker downloads both images.

```powershell
dotnet test --project .\test\Serilog.Sinks.OpenSearch.IntegrationTests\Serilog.Sinks.OpenSearch.IntegrationTests.csproj --configuration Release
```

To run the generated Microsoft Testing Platform executable directly:

```powershell
.\test\Serilog.Sinks.OpenSearch.IntegrationTests\bin\Release\net10.0\Serilog.Sinks.OpenSearch.IntegrationTests.exe
```
