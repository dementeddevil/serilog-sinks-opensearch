using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextSinkOptionsTests
    {
        [Test]
        public async Task SinkOptionsDefaultTrueAndFactoriesPropagate()
        {
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200")) { DetectOpenSearchVersion = false };
            await Assert.That(options.RenderTraceId).IsTrue();
            await Assert.That(options.RenderSpanId).IsTrue();
            options.RenderTraceId = false;
            var logEvent = TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId);
            var normal = TraceContextTestHelper.Format(OpenSearchSinkState.CreateDefaultFormatter(options), logEvent);
            var durable = TraceContextTestHelper.Format(OpenSearchSinkState.CreateDefaultDurableFormatter(options), logEvent);
            await Assert.That(normal).DoesNotContain("\"TraceId\"");
            await Assert.That(normal).Contains("\"SpanId\"");
            await Assert.That(durable).DoesNotContain("\"TraceId\"");
        }

        [Test]
        public async Task LongConfigurationOverloadAcceptsAppendedNamedControls()
        {
            using var logger = new LoggerConfiguration().WriteTo.OpenSearch("http://localhost:9200",
                detectOpenSearchVersion: false, renderTraceId: false, renderSpanId: false).CreateLogger();
            using var existingCall = new LoggerConfiguration().WriteTo.OpenSearch("http://localhost:9200",
                detectOpenSearchVersion: false).CreateLogger();
            await Assert.That(logger).IsNotNull();
        }
    }
}
