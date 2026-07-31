using System;
using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Client;
using Serilog.Events;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using TUnit.Assertions;

namespace Serilog.Sinks.OpenSearch.IntegrationTests
{
    internal static class OpenSearchIntegrationScenarios
    {
        private static readonly string[] ExpectedLevels =
        {
            nameof(LogEventLevel.Information),
            nameof(LogEventLevel.Warning),
            nameof(LogEventLevel.Error),
            nameof(LogEventLevel.Fatal),
        };

        public static Task ExplicitTemplateVersion(OpenSearchFixture fixture) =>
            Run(fixture, detectVersion: false, "explicit-template");

        public static Task DetectedTemplateVersion(OpenSearchFixture fixture) =>
            Run(fixture, detectVersion: true, "detected-template");

        private static async Task Run(OpenSearchFixture fixture, bool detectVersion, string scenario)
        {
            var suffix = Guid.NewGuid().ToString("N");
            var indexPrefix = $"logs-os{fixture.ExpectedMajorVersion}-{scenario}-{suffix}";
            var templateName = $"serilog-os{fixture.ExpectedMajorVersion}-{scenario}-{suffix}";

            var options = OpenSearchSinkOptionsFactory.Create(
                fixture.Endpoint,
                indexPrefix,
                templateName,
                o =>
                {
                    o.DetectOpenSearchVersion = detectVersion;
                    o.AutoRegisterTemplateVersion = detectVersion ? null : AutoRegisterTemplateVersion.OSv1;
                });

            using (var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.OpenSearch(options)
                .CreateLogger())
            {
                logger.Information("Hello Information");
                logger.Debug("Hello Debug");
                logger.Warning("Hello Warning");
                logger.Error("Hello Error");
                logger.Fatal("Hello Fatal");
            }

            var templateResponse = await fixture.Client.Indices.GetTemplateAsync(templateName).ConfigureAwait(false);
            await Assert.That(templateResponse.IsValid)
                .IsTrue()
                .Because(templateResponse.DebugInformation);
            await Assert.That(templateResponse.TemplateMappings.Keys).Contains(templateName);

            var template = templateResponse.TemplateMappings[templateName];
            await Assert.That(template.IndexPatterns.Any(pattern => pattern.StartsWith(indexPrefix, StringComparison.Ordinal)))
                .IsTrue();

            var indexPattern = indexPrefix + "-*";
            var refreshResponse = await fixture.Client.Indices.RefreshAsync(indexPattern).ConfigureAwait(false);
            await Assert.That(refreshResponse.IsValid)
                .IsTrue()
                .Because(refreshResponse.DebugInformation);

            var searchResponse = await fixture.Client.SearchAsync<LogDocument>(s => s
                    .Index(indexPattern)
                    .Size(10)
                    .Query(q => q.MatchAll()))
                .ConfigureAwait(false);

            await Assert.That(searchResponse.IsValid)
                .IsTrue()
                .Because(searchResponse.DebugInformation);
            await Assert.That(searchResponse.Documents).Count().IsEqualTo(4);
            await Assert.That(searchResponse.Documents.Select(document => document.Level))
                .IsEquivalentTo(ExpectedLevels);
            await Assert.That(searchResponse.Documents.Select(document => document.RenderedMessage))
                .DoesNotContain("Hello Debug");
        }

        private sealed class LogDocument
        {
            public string Level { get; set; }

            public string RenderedMessage { get; set; }
        }
    }
}
