using System.Linq;
using Elastic.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using Serilog.Sinks.OpenSearch.IntegrationTests.OpenSearch7.Bootstrap;
using Xunit;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.OpenSearch7
{
    public class OpenSearch7XUsing6X : OpenSearch7XTestBase, IClassFixture<OpenSearch7XUsing6X.SetupSerilog>
    {
        private readonly SetupSerilog _setup;

        public OpenSearch7XUsing6X(OpenSearch7XCluster cluster, SetupSerilog setup) : base(cluster) => _setup = setup;

        [I] public void AssertTemplate()
        {
            var templateResponse = Client.Indices.GetTemplate(SetupSerilog.TemplateName);
            templateResponse.TemplateMappings.Should().NotBeEmpty();
            templateResponse.TemplateMappings.Keys.Should().Contain(SetupSerilog.TemplateName);

            var template = templateResponse.TemplateMappings[SetupSerilog.TemplateName];

            template.IndexPatterns.Should().Contain(pattern => pattern.StartsWith(SetupSerilog.IndexPrefix));
        }

        [I] public void AssertLogs()
        {
            var refreshed = Client.Indices.Refresh(SetupSerilog.IndexPrefix + "*");

            var search = Client.Search<object>(s => s.Index(SetupSerilog.IndexPrefix + "*"));

            // Informational should be filtered
            search.Documents.Count().Should().Be(4);
        }
        
        // ReSharper disable once ClassNeverInstantiated.Global
        public class SetupSerilog
        {
            public const string IndexPrefix = "logs-7x-using-6x";
            public const string TemplateName = "serilog-logs-7x-using-6x";

            public SetupSerilog()
            {
                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.OpenSearch(
                        OpenSearchSinkOptionsFactory.Create(IndexPrefix, TemplateName, o =>
                        {
                            o.DetectOpenSearchVersion = true;
                            o.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6;
                            o.AutoRegisterTemplate = true;
                        })
                    );
                using (var logger = loggerConfig.CreateLogger())
                {
                    logger.Information("Hello Information");
                    logger.Debug("Hello Debug");
                    logger.Warning("Hello Warning");
                    logger.Error("Hello Error");
                    logger.Fatal("Hello Fatal");
                }
            }
        }

    }
}