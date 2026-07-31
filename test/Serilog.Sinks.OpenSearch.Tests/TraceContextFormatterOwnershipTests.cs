using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextFormatterOwnershipTests
    {
        [Test]
        public async Task CustomNormalAndDurableFormattersOwnOutputAndReceiveSameEvent()
        {
            var normal = new RecordingTextFormatter();
            var durable = new RecordingTextFormatter();
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { CustomFormatter = normal, CustomDurableFormatter = durable, DetectOpenSearchVersion = false };
            var state = OpenSearchSinkState.Create(options);
            var logEvent = TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId);
            await Assert.That(TraceContextTestHelper.Format(state.Formatter, logEvent)).IsEqualTo(RecordingTextFormatter.Marker);
            await Assert.That(TraceContextTestHelper.Format(state.DurableFormatter, logEvent)).IsEqualTo(RecordingTextFormatter.Marker);
            await Assert.That(normal.Events.Single()).IsSameReferenceAs(logEvent);
            await Assert.That(durable.Events.Single()).IsSameReferenceAs(logEvent);
        }

        [Test]
        public async Task CompactFormatterOutputMatchesDirectFormattingExactly()
        {
            var compact = new CompactJsonFormatter();
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200")) { CustomFormatter = compact, DetectOpenSearchVersion = false };
            var logEvent = TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId);
            var direct = TraceContextTestHelper.Format(compact, logEvent);
            var selected = TraceContextTestHelper.Format(OpenSearchSinkState.Create(options).Formatter, logEvent);
            await Assert.That(selected).IsEqualTo(direct);
        }
    }
}
