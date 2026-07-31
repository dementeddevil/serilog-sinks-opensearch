using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.OpenSearch.Tests.Stubs
{
    internal sealed class RecordingFailureListener : ILoggingFailureListener
    {
        public ConcurrentQueue<Failure> Failures { get; } = new();

        public void OnLoggingFailed(object source, LoggingFailureKind kind, string message,
            IReadOnlyCollection<LogEvent> events, Exception exception)
            => Failures.Enqueue(new Failure(source, kind, message, events, exception));

        internal sealed record Failure(object Source, LoggingFailureKind Kind, string Message,
            IReadOnlyCollection<LogEvent> Events, Exception Exception);
    }
}
