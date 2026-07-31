using System.Threading.Tasks;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using TUnit.Core;

namespace Serilog.Sinks.OpenSearch.IntegrationTests
{
    [ClassDataSource<OpenSearch1Fixture>(Shared = SharedType.PerClass)]
    [NotInParallel]
    public sealed class OpenSearch1Tests(OpenSearch1Fixture fixture)
    {
        [Test]
        public Task ExplicitTemplateVersionRegistersTemplateAndIndexesLogs() =>
            OpenSearchIntegrationScenarios.ExplicitTemplateVersion(fixture);

        [Test]
        public Task DetectedVersionRegistersTemplateAndIndexesLogs() =>
            OpenSearchIntegrationScenarios.DetectedTemplateVersion(fixture);

        [Test]
        public Task IndexedDocumentsPreserveTraceContext() =>
            TraceContextIntegrationScenarios.IndexedDocumentsPreserveTraceContext(fixture);
    }

    [ClassDataSource<OpenSearch2Fixture>(Shared = SharedType.PerClass)]
    [NotInParallel]
    public sealed class OpenSearch2Tests(OpenSearch2Fixture fixture)
    {
        [Test]
        public Task ExplicitTemplateVersionRegistersTemplateAndIndexesLogs() =>
            OpenSearchIntegrationScenarios.ExplicitTemplateVersion(fixture);

        [Test]
        public Task DetectedVersionRegistersTemplateAndIndexesLogs() =>
            OpenSearchIntegrationScenarios.DetectedTemplateVersion(fixture);

        [Test]
        public Task IndexedDocumentsPreserveTraceContext() =>
            TraceContextIntegrationScenarios.IndexedDocumentsPreserveTraceContext(fixture);
    }
}
