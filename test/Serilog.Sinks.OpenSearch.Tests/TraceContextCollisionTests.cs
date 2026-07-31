using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextCollisionTests
    {
        [Test]
        [Arguments(false, "TraceId")]
        [Arguments(false, "traceid")]
        [Arguments(true, "TrAcEiD")]
        [Arguments(true, "SPANID")]
        public async Task FirstClassIdentifiersWinCaseInsensitiveCollisions(bool inline, string name)
        {
            var properties = new[] { new LogEventProperty(name, new ScalarValue("user-value")), new LogEventProperty("Ordinary", new ScalarValue(42)) };
            var output = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(inlineFields: inline),
                TraceContextTestHelper.CreateEvent(properties, TraceContextTestHelper.TraceId, TraceContextTestHelper.SpanId));
            using var json = TraceContextTestHelper.Parse(output);
            await Assert.That(json.RootElement.GetProperty("TraceId").GetString()).IsEqualTo(TraceContextTestHelper.TraceIdText);
            await Assert.That(json.RootElement.GetProperty("SpanId").GetString()).IsEqualTo(TraceContextTestHelper.SpanIdText);
            await Assert.That(output).DoesNotContain("user-value");
            await Assert.That(output).Contains("Ordinary");
        }

        [Test]
        [Arguments(false, false)]
        [Arguments(true, false)]
        [Arguments(false, true)]
        public async Task UserPropertiesRemainWhenIdentifierUnavailableOrDisabled(bool inline, bool disabled)
        {
            var properties = new[] { new LogEventProperty("traceid", new ScalarValue("user-trace")), new LogEventProperty("SpanId", new ScalarValue("user-span")) };
            var formatter = new OpenSearchJsonFormatter(inlineFields: inline, renderTraceId: !disabled, renderSpanId: !disabled);
            var output = TraceContextTestHelper.Format(formatter, TraceContextTestHelper.CreateEvent(properties));
            await Assert.That(output).Contains("user-trace");
            await Assert.That(output).Contains("user-span");
        }

        [Test]
        public async Task AllCollidingPropertiesDoNotCreateEmptyFieldsContainer()
        {
            var properties = new[] { new LogEventProperty("TRACEID", new StructureValue(new List<LogEventProperty>())), new LogEventProperty("spanid", new ScalarValue("x")) };
            var output = TraceContextTestHelper.Format(new OpenSearchJsonFormatter(),
                TraceContextTestHelper.CreateEvent(properties, TraceContextTestHelper.TraceId, TraceContextTestHelper.SpanId));
            await Assert.That(output).DoesNotContain("\"fields\"");
        }
    }
}
