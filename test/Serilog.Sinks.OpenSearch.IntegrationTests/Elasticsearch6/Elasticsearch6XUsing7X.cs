using System.Linq;
using Elastic.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using Serilog.Sinks.OpenSearch.IntegrationTests.OpenSearch6.Bootstrap;
using Xunit;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.OpenSearch6
{
    public class OpenSearch6XUsing7X : OpenSearch6XTestBase, IClassFixture<OpenSearch6XUsing7X.SetupSerilog>
    {
        private readonly SetupSerilog _setup;

        public OpenSearch6XUsing7X(OpenSearch6XCluster cluster, SetupSerilog setup) : base(cluster) => _setup = setup;

        [I] public void AssertTemplate()
        {
            var templateResponse = Client.GetIndexTemplate(t=>t.Name(SetupSerilog.TemplateName));
            templateResponse.TemplateMappings.Should().NotBeEmpty();
            templateResponse.TemplateMappings.Keys.Should().Contain(SetupSerilog.TemplateName);

            var template = templateResponse.TemplateMappings[SetupSerilog.TemplateName];

            template.IndexPatterns.Should().Contain(pattern => pattern.StartsWith(SetupSerilog.IndexPrefix));
        }

        [I] public void AssertLogs()
        {
            var refreshed = Client.Refresh(SetupSerilog.IndexPrefix + "*");

            var search = Client.Search<object>(s => s
                .Index(SetupSerilog.IndexPrefix + "*")
                .Type("_doc")
            );

            // Informational should be filtered
            search.Documents.Count().Should().Be(4);
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class SetupSerilog
        {
            public const string IndexPrefix = "logs-6x-using-7x-";
            public const string TemplateName = "serilog-logs-6x-using-7x";

            public SetupSerilog()
            {
                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.OpenSearch(
                        OpenSearchSinkOptionsFactory.Create(IndexPrefix, TemplateName, o =>
                        {
                            o.DetectOpenSearchVersion = true;
                            o.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7;
                            o.AutoRegisterTemplate = true;
                        })
                    );
                var logger = loggerConfig.CreateLogger();

                logger.Information("Hello Information");
                logger.Debug("Hello Debug");
                logger.Warning("Hello Warning");
                logger.Error("Hello Error");
                logger.Fatal("Hello Fatal");

                logger.Dispose();
            }
        }
    }
}