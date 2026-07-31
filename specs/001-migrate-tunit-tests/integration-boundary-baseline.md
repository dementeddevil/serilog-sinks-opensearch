# Deferred Integration Boundary Baseline

**Captured**: 2026-07-31 before unit-test migration edits

The integration project is outside this feature. Existing user changes are preserved rather than reverted, reformatted, or completed here.

## Existing Working-Tree State

Tracked modifications already present:

- `serilog-sinks-opensearch.sln`
- `test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/OpenSearchSinkOptionsFactory.cs`
- `test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/SerilogSinkOpenSearchXunitRunOptions.cs`
- `test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/XunitBootstrap.cs`
- `test/Serilog.Sinks.OpenSearch.IntegrationTests/Serilog.Sinks.OpenSearch.IntegrationTests.csproj`

The existing diff contained 28 insertions and 23 deletions across these five files. The integration project was a member of `serilog-sinks-opensearch.sln` at capture time. Its project file also contained known pre-existing trailing whitespace.

## SHA-256 Source Baseline

Generated `bin/` and `obj/` files are intentionally excluded.

```text
445AC2CF3DCB2840B3DCFA732CCE226F1206973C7C04D69CFC90BDE14ACC9EF6  test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/ClientTestClusterBase.cs
FBFB48E423A5213222A6C2BE0E61F5970071A336C82B2C46588370B8842E1D05  test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/OpenSearchSinkOptionsFactory.cs
740AF02478712B6A8D520694BCC25EEED05855DF7357D824248D1DE95129DEE7  test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/ProxyDetection.cs
F16401444B71C192A015A4E35CAA2C4C8BD186185C4E3DB2390C1F8650A2B892  test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/SerilogSinkOpenSearchXunitRunOptions.cs
868D0A0F83D2F4C6FBDECDDFBE984153459142FE81B55D133E061BAB521CBC63  test/Serilog.Sinks.OpenSearch.IntegrationTests/Bootstrap/XunitBootstrap.cs
8DD05402486F391DBCDED3D6035F9C2AE86170B50688012FAC7DD8BAAC8A49DA  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch6/Bootstrap/Elasticsearch6XCluster.cs
57879527C0BDB71EE83A49476BDC6563A42FD8D49AC83BC0324BB84A60AD6B98  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch6/Bootstrap/Elasticsearch6XTestBase.cs
E9FD5EB253BDA06B987936F4C4CC4AC1005FD51FF6C4FDB7559E900944D0C247  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch6/Elasticsearch6X.cs
A82B335968C69FF7ED8DA743E101C59005FEDADA6A13D49F7BE979685780576F  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch6/Elasticsearch6XUsing7X.cs
944CD85CB9D930BFB7B1AFC1CB83251FCDF8FC0CA9234FCB3C9EDC23CD002D38  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch7/Bootstrap/Elasticsearch7XCluster.cs
4E7DD4317F2115FF342386A70228867484BC95E851897166EF632F5F21724452  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch7/Bootstrap/Elasticsearch7XTestBase.cs
6E421AF2A5A07D09DEA6E24101E2DE4EB4DC92E9810BC09BE3FE79D6D6C08F38  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch7/Elasticsearch7X.cs
062D9AF9FEF184A00C2E2562538A85931E145725E2AD9BBAB7D22E0D1E628364  test/Serilog.Sinks.OpenSearch.IntegrationTests/Elasticsearch7/Elasticsearch7XUsing6X.cs
33BB0B3373F5814B8E6FB666A0DD9C17573E05378153730ED130AB32B8786D0A  test/Serilog.Sinks.OpenSearch.IntegrationTests/Serilog.Sinks.OpenSearch.IntegrationTests.csproj
BC9F5960614E849EAB1432CECAE938AC99A3D24BFDFAF08E4CE858023BF91E66  serilog-sinks-opensearch.sln
```

## Completion Rule

At feature completion, recompute this source-only hash set and confirm it is identical. Any concurrent user edit must be recorded as a new user-owned baseline event; Codex must not overwrite it.
