using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextFormattingTests
    {
        [Test]
        public async Task CapturedActivityContextIsSerializedAfterActivityStops()
        {
            var logEvent = TraceContextTestHelper.CaptureWithLogger(true, out var activity);
            await Assert.That(Activity.Current).IsNotEqualTo(activity);
            using var json = TraceContextTestHelper.Parse(TraceContextTestHelper.Format(new OpenSearchJsonFormatter(), logEvent));
            await Assert.That(json.RootElement.GetProperty("TraceId").GetString()).IsEqualTo(activity.TraceId.ToString());
            await Assert.That(json.RootElement.GetProperty("SpanId").GetString()).IsEqualTo(activity.SpanId.ToString());
        }

        [Test]
        public async Task NoActivityOmitsTraceContext()
        {
            var logEvent = TraceContextTestHelper.CaptureWithLogger(false, out _);
            var output = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(), logEvent);
            await Assert.That(output).DoesNotContain("\"TraceId\"");
            await Assert.That(output).DoesNotContain("\"SpanId\"");
        }

        [Test]
        public async Task ExplicitIdsAreCanonicalAndDefaultOn()
        {
            var logEvent = TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId);
            using var json = TraceContextTestHelper.Parse(TraceContextTestHelper.Format(new OpenSearchJsonFormatter(), logEvent));
            await Assert.That(json.RootElement.GetProperty("TraceId").GetString()).IsEqualTo(TraceContextTestHelper.TraceIdText);
            await Assert.That(json.RootElement.GetProperty("SpanId").GetString()).IsEqualTo(TraceContextTestHelper.SpanIdText);
            await Assert.That(TraceContextTestHelper.TraceIdText.Length).IsEqualTo(32);
            await Assert.That(TraceContextTestHelper.SpanIdText.Length).IsEqualTo(16);
        }

        [Test]
        public async Task ConsecutiveActivitiesRetainTheirCapturedSpan()
        {
            var first = TraceContextTestHelper.CaptureWithLogger(true, out var firstActivity);
            var second = TraceContextTestHelper.CaptureWithLogger(true, out var secondActivity);
            await Assert.That(firstActivity.SpanId).IsNotEqualTo(secondActivity.SpanId);
            await Assert.That(TraceContextTestHelper.Format(new OpenSearchJsonFormatter(), first)).Contains(firstActivity.SpanId.ToString());
            await Assert.That(TraceContextTestHelper.Format(new OpenSearchJsonFormatter(), second)).Contains(secondActivity.SpanId.ToString());
        }

        [Test]
        [Arguments(true, false)]
        [Arguments(false, true)]
        public async Task IndependentAvailability(bool trace, bool span)
        {
            var logEvent = TraceContextTestHelper.CreateEvent(traceId: trace ? TraceContextTestHelper.TraceId : default,
                spanId: span ? TraceContextTestHelper.SpanId : default);
            var output = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(), logEvent);
            await Assert.That(output.Contains("\"TraceId\"", StringComparison.Ordinal)).IsEqualTo(trace);
            await Assert.That(output.Contains("\"SpanId\"", StringComparison.Ordinal)).IsEqualTo(span);
        }

        [Test]
        public async Task FragmentsAndCustomDelimiterRemainValid()
        {
            var logEvent = TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId);
            var fragment = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(omitEnclosingObject: true), logEvent);
            using var fragmentJson = TraceContextTestHelper.Parse(fragment, fragment: true);
            await Assert.That(fragmentJson.RootElement.GetProperty("TraceId").GetString()).IsEqualTo(TraceContextTestHelper.TraceIdText);
            var delimited = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(closingDelimiter: "END"), logEvent);
            await Assert.That(delimited).EndsWith("END");
        }

        [Test]
        public async Task ExceptionFormatterIncludesTraceContext()
        {
            var output = TraceContextTestHelper.Format(new ExceptionAsObjectJsonFormatter(),
                TraceContextTestHelper.CreateEvent(traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId));
            await Assert.That(output).Contains("\"TraceId\":\"" + TraceContextTestHelper.TraceIdText + "\"");
            await Assert.That(output).Contains("\"SpanId\":\"" + TraceContextTestHelper.SpanIdText + "\"");
        }
    }
}
