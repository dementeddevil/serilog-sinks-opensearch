using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class TraceContextFormatterBenchmarkTests
    {
        [Test]
        public async Task TraceContextThroughputRegressionIsWithinFivePercent()
        {
            var events = Enumerable.Range(0, 20000).Select(_ => TraceContextTestHelper.CreateEvent(
                traceId: TraceContextTestHelper.TraceId, spanId: TraceContextTestHelper.SpanId)).ToArray();
            var baseline = new OpenSearchJsonFormatter(renderTraceId: false, renderSpanId: false);
            var candidate = new OpenSearchJsonFormatter();
            Measure(baseline, events, 1000);
            Measure(candidate, events, 1000);
            var baselineSamples = new double[7];
            var candidateSamples = new double[7];
            for (var i = 0; i < 7; i++)
            {
                baselineSamples[i] = Measure(baseline, events, events.Length);
                candidateSamples[i] = Measure(candidate, events, events.Length);
            }
            Array.Sort(baselineSamples);
            Array.Sort(candidateSamples);
            var ratio = candidateSamples[3] / baselineSamples[3];
            var stable = baselineSamples[6] / baselineSamples[0] <= 1.50 && candidateSamples[6] / candidateSamples[0] <= 1.50;
            var report = $"Runtime={Environment.Version}; BaselineMs={string.Join(',', baselineSamples.Select(v => v.ToString("F3")))}; CandidateMs={string.Join(',', candidateSamples.Select(v => v.ToString("F3")))}; Ratio={ratio:F4}; IdleGuard={(stable ? "PASS" : "FAIL")}";
            Console.WriteLine(report);
            Console.Error.WriteLine(report);
            if (!stable)
                return;
            await Assert.That(ratio).IsLessThanOrEqualTo(1.05);
        }

        static double Measure(OpenSearchJsonFormatter formatter, Serilog.Events.LogEvent[] events, int count)
        {
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < count; i++) TraceContextTestHelper.Format(formatter, events[i % events.Length]);
            watch.Stop();
            return watch.Elapsed.TotalMilliseconds;
        }
    }
}
