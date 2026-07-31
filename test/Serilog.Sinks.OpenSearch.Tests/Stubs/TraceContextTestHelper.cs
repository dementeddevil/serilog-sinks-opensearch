using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;

namespace Serilog.Sinks.OpenSearch.Tests.Stubs
{
    internal static class TraceContextTestHelper
    {
        public const string TraceIdText = "4bf92f3577b34da6a3ce929d0e0e4736";
        public const string SpanIdText = "00f067aa0ba902b7";

        public static ActivityTraceId TraceId => ActivityTraceId.CreateFromString(TraceIdText.AsSpan());
        public static ActivitySpanId SpanId => ActivitySpanId.CreateFromString(SpanIdText.AsSpan());

        public static LogEvent CreateEvent(
            IEnumerable<LogEventProperty> properties = null,
            ActivityTraceId traceId = default,
            ActivitySpanId spanId = default,
            string template = "Message {Value:000}")
        {
            var parsed = new MessageTemplateParser().Parse(template);
            return new LogEvent(DateTimeOffset.Parse("2026-01-02T03:04:05+00:00"), LogEventLevel.Information,
                null, parsed, properties ?? Enumerable.Empty<LogEventProperty>(), traceId, spanId);
        }

        public static LogEvent CaptureWithLogger(bool insideActivity, out Activity activity)
        {
            var sink = new CollectingSink();
            using var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
            activity = null;
            var previousActivity = Activity.Current;
            if (insideActivity)
            {
                activity = new Activity("trace-context-test").SetIdFormat(ActivityIdFormat.W3C).Start();
            }
            else
            {
                Activity.Current = null;
            }
            logger.Information("Captured");
            activity?.Stop();
            Activity.Current = previousActivity;
            return sink.Events.Single();
        }

        public static string Format(ITextFormatter formatter, LogEvent logEvent)
        {
            using var writer = new StringWriter();
            formatter.Format(logEvent, writer);
            return writer.ToString();
        }

        public static JsonDocument Parse(string output, bool fragment = false)
        {
            var json = fragment ? "{" + output + "}" : output.Trim();
            return JsonDocument.Parse(json);
        }
    }
}
