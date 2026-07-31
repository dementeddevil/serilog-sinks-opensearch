using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextRenderingTests
    {
        [Test]
        [Arguments(false)]
        [Arguments(true)]
        public async Task SuppressedCollisionStillRendersFromOriginalEvent(bool inline)
        {
            var property = new LogEventProperty("TraceId", new ScalarValue("user-trace"));
            var logEvent = TraceContextTestHelper.CreateEvent(new[] { property }, TraceContextTestHelper.TraceId,
                TraceContextTestHelper.SpanId, "Trace {TraceId:U}");
            var output = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(inlineFields: inline), logEvent);
            await Assert.That(output).Contains("user-trace");
            await Assert.That(output).Contains("renderings");
            await Assert.That(logEvent.Properties["TraceId"]).IsSameReferenceAs(property.Value);
        }
    }
}
