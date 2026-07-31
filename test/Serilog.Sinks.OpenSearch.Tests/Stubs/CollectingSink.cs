using System;
using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.OpenSearch.Tests.Stubs
{
    internal sealed class CollectingSink : ILogEventSink
    {
        private readonly Exception _failure;
        public ConcurrentQueue<LogEvent> Events { get; } = new();

        public CollectingSink(Exception failure = null) => _failure = failure;

        public void Emit(LogEvent logEvent)
        {
            if (_failure != null) throw _failure;
            Events.Enqueue(logEvent);
        }
    }
}
