using Elastic.OpenSearch.Xunit;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using Xunit;

[assembly: TestFramework("Elastic.OpenSearch.Xunit.Sdk.ElasticTestFramework", "Elastic.OpenSearch.Xunit")]
[assembly: ElasticXunitConfiguration(typeof(SerilogSinkOpenSearchXunitRunOptions))]
