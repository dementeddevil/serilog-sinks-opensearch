using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextConcurrencyTests
    {
        [Test]
        public async Task FormatsTenThousandMixedEventsConcurrently()
        {
            var formatter = new OpenSearchJsonFormatter();
            var failures = new ConcurrentQueue<string>();
            Parallel.For(0, 10000, index =>
            {
                var traced = index % 2 == 0;
                var traceId = traced ? ActivityTraceId.CreateRandom() : default;
                var spanId = traced ? ActivitySpanId.CreateRandom() : default;
                using var json = TraceContextTestHelper.Parse(TraceContextTestHelper.Format(formatter,
                    TraceContextTestHelper.CreateEvent(traceId: traceId, spanId: spanId)));
                var root = json.RootElement;
                if (traced && (root.GetProperty("TraceId").GetString() != traceId.ToString() || root.GetProperty("SpanId").GetString() != spanId.ToString()))
                    failures.Enqueue(index.ToString());
                if (!traced && (root.TryGetProperty("TraceId", out _) || root.TryGetProperty("SpanId", out _)))
                    failures.Enqueue(index.ToString());
            });
            await Assert.That(failures).IsEmpty();
        }
    }
}
