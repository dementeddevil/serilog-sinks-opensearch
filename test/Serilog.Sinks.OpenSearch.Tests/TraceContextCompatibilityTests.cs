using System.Threading.Tasks;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextCompatibilityTests
    {
        [Test]
        [Arguments(true, true)]
        [Arguments(true, false)]
        [Arguments(false, true)]
        [Arguments(false, false)]
        public async Task FormatterControlsAreIndependent(bool renderTrace, bool renderSpan)
        {
            var formatter = new OpenSearchJsonFormatter(renderTraceId: renderTrace, renderSpanId: renderSpan);
            var output = TraceContextTestHelper.Format(formatter,
                TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId));
            await Assert.That(output.Contains("\"TraceId\"")).IsEqualTo(renderTrace);
            await Assert.That(output.Contains("\"SpanId\"")).IsEqualTo(renderSpan);
        }

        [Test]
        public async Task ExistingConstructorsAndPublicConstantsRemainUsable()
        {
            _ = new OpenSearchJsonFormatter(false, null, true, null, null, false, true, false, false);
            _ = new ExceptionAsObjectJsonFormatter(false, null, false, null, null, false, true, false);
            await Assert.That(OpenSearchJsonFormatter.TraceIdPropertyName).IsEqualTo("TraceId");
            await Assert.That(OpenSearchJsonFormatter.SpanIdPropertyName).IsEqualTo("SpanId");
        }
    }
}
