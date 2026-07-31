using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Client;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using TUnit.Assertions;

namespace Serilog.Sinks.OpenSearch.IntegrationTests
{
    internal static class TraceContextIntegrationScenarios
    {
        public static async Task IndexedDocumentsPreserveTraceContext(OpenSearchFixture fixture)
        {
            var suffix = Guid.NewGuid().ToString("N");
            var nestedIndexPrefix = $"trace-os{fixture.ExpectedMajorVersion}-nested-{suffix}";
            var inlineIndexPrefix = $"trace-os{fixture.ExpectedMajorVersion}-inline-{suffix}";

            await VerifyNestedDefaults(fixture, nestedIndexPrefix, $"trace-nested-{suffix}")
                .ConfigureAwait(false);
            await VerifyInlineOptOut(fixture, inlineIndexPrefix, $"trace-inline-{suffix}")
                .ConfigureAwait(false);
        }

        private static async Task VerifyNestedDefaults(
            OpenSearchFixture fixture,
            string indexPrefix,
            string templateName)
        {
            var options = OpenSearchSinkOptionsFactory.Create(
                fixture.Endpoint,
                indexPrefix,
                templateName,
                configured =>
                {
                    configured.DetectOpenSearchVersion = false;
                    configured.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1;
                });

            string expectedTraceId;
            string expectedSpanId;
            var previousActivity = Activity.Current;
            Activity.Current = null;
            try
            {
                using var logger = new LoggerConfiguration()
                    .WriteTo.OpenSearch(options)
                    .CreateLogger();

                using (var activity = new Activity("indexed-trace-context")
                    .SetIdFormat(ActivityIdFormat.W3C)
                    .Start())
                {
                    expectedTraceId = activity.TraceId.ToString();
                    expectedSpanId = activity.SpanId.ToString();

                    logger
                        .ForContext("TraceId", "shadow-trace")
                        .ForContext("SpanId", new { Shadow = true }, destructureObjects: true)
                        .ForContext("Ordinary", "kept")
                        .Information("Trace integration {Scenario}", "traced");
                }

                logger
                    .ForContext("TraceId", "fallback-trace")
                    .ForContext("SpanId", "fallback-span")
                    .ForContext("Ordinary", "also-kept")
                    .Information("Trace integration {Scenario}", "untraced");
            }
            finally
            {
                Activity.Current = previousActivity;
            }

            var documents = await ReadDocuments(fixture, indexPrefix).ConfigureAwait(false);
            await Assert.That(documents).Count().IsEqualTo(2);

            var traced = documents.Single(document => document.Fields?.Scenario == "traced");
            await Assert.That(traced.TraceId).IsEqualTo(expectedTraceId);
            await Assert.That(traced.SpanId).IsEqualTo(expectedSpanId);
            await Assert.That(traced.TraceId).Length().IsEqualTo(32);
            await Assert.That(traced.SpanId).Length().IsEqualTo(16);
            await Assert.That(traced.Fields.TraceId).IsNull();
            await Assert.That(traced.Fields.SpanId).IsNull();
            await Assert.That(traced.Fields.Ordinary).IsEqualTo("kept");

            var untraced = documents.Single(document => document.Fields?.Scenario == "untraced");
            await Assert.That(untraced.TraceId).IsNull();
            await Assert.That(untraced.SpanId).IsNull();
            await Assert.That(untraced.Fields.TraceId).IsEqualTo("fallback-trace");
            await Assert.That(untraced.Fields.SpanId).IsEqualTo("fallback-span");
            await Assert.That(untraced.Fields.Ordinary).IsEqualTo("also-kept");
        }

        private static async Task VerifyInlineOptOut(
            OpenSearchFixture fixture,
            string indexPrefix,
            string templateName)
        {
            var options = OpenSearchSinkOptionsFactory.Create(
                fixture.Endpoint,
                indexPrefix,
                templateName,
                configured =>
                {
                    configured.DetectOpenSearchVersion = false;
                    configured.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1;
                    configured.InlineFields = true;
                    configured.RenderTraceId = false;
                    configured.RenderSpanId = true;
                });

            string expectedSpanId;
            var previousActivity = Activity.Current;
            Activity.Current = null;
            try
            {
                using var logger = new LoggerConfiguration()
                    .WriteTo.OpenSearch(options)
                    .CreateLogger();

                using var activity = new Activity("indexed-inline-trace-context")
                    .SetIdFormat(ActivityIdFormat.W3C)
                    .Start();
                expectedSpanId = activity.SpanId.ToString();

                logger
                    .ForContext("TraceId", "fallback-inline-trace")
                    .ForContext("SpanId", "shadow-inline-span")
                    .ForContext("Ordinary", "inline-kept")
                    .Information("Trace integration {Scenario}", "inline");
            }
            finally
            {
                Activity.Current = previousActivity;
            }

            var documents = await ReadDocuments(fixture, indexPrefix).ConfigureAwait(false);
            await Assert.That(documents).Count().IsEqualTo(1);

            var document = documents.Single();
            await Assert.That(document.Scenario).IsEqualTo("inline");
            await Assert.That(document.TraceId).IsEqualTo("fallback-inline-trace");
            await Assert.That(document.SpanId).IsEqualTo(expectedSpanId);
            await Assert.That(document.Ordinary).IsEqualTo("inline-kept");
            await Assert.That(document.Fields).IsNull();
        }

        private static async Task<TraceLogDocument[]> ReadDocuments(
            OpenSearchFixture fixture,
            string indexPrefix)
        {
            var indexPattern = indexPrefix + "-*";
            var refreshResponse = await fixture.Client.Indices.RefreshAsync(indexPattern).ConfigureAwait(false);
            await Assert.That(refreshResponse.IsValid)
                .IsTrue()
                .Because(refreshResponse.DebugInformation);

            var searchResponse = await fixture.Client.SearchAsync<TraceLogDocument>(search => search
                    .Index(indexPattern)
                    .Size(10)
                    .Query(query => query.MatchAll()))
                .ConfigureAwait(false);
            await Assert.That(searchResponse.IsValid)
                .IsTrue()
                .Because(searchResponse.DebugInformation);

            return searchResponse.Documents.ToArray();
        }

        private sealed class TraceLogDocument
        {
            [PropertyName("TraceId")]
            public string TraceId { get; set; }

            [PropertyName("SpanId")]
            public string SpanId { get; set; }

            [PropertyName("Scenario")]
            public string Scenario { get; set; }

            [PropertyName("Ordinary")]
            public string Ordinary { get; set; }

            [PropertyName("fields")]
            public TraceFields Fields { get; set; }
        }

        private sealed class TraceFields
        {
            [PropertyName("TraceId")]
            public string TraceId { get; set; }

            [PropertyName("SpanId")]
            public string SpanId { get; set; }

            [PropertyName("Scenario")]
            public string Scenario { get; set; }

            [PropertyName("Ordinary")]
            public string Ordinary { get; set; }
        }
    }
}
