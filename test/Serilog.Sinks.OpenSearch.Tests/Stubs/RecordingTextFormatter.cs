using System.Collections.Concurrent;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.OpenSearch.Tests.Stubs
{
    internal sealed class RecordingTextFormatter : ITextFormatter
    {
        public const string Marker = "CUSTOM-FORMATTER";
        public ConcurrentQueue<LogEvent> Events { get; } = new();

        public void Format(LogEvent logEvent, TextWriter output)
        {
            Events.Enqueue(logEvent);
            output.Write(Marker);
        }
    }
}
