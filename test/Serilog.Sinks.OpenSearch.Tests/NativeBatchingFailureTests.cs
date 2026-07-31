using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.OpenSearch.Tests.Stubs;
using Serilog.Core;
using System.IO;
using Serilog.Debugging;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class NativeBatchingFailureTests
    {
        [Test]
        public async Task WholeBatchFailureDoesNotInvokeLegacyFailureSink()
        {
            var connection = new ScriptedConnection();
            connection.EnqueueException(new InvalidOperationException("failed"));
            var legacy = new CollectingSink();
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            {
                Connection = connection,
                DetectOpenSearchVersion = false,
                FailureSink = legacy,
                EmitEventFailure = EmitEventFailureHandling.WriteToFailureSink
            };
            try { await new BatchedOpenSearchSink(options).EmitBatchAsync(new[] { Event() }); } catch { }
            await Assert.That(legacy.Events).IsEmpty();
        }

        [Test]
        public async Task ZeroRetryReportsPermanentFailureAndFallbackReceivesEvent()
        {
            var connection = new ScriptedConnection();
            connection.EnqueueException(new InvalidOperationException("failed"));
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { Connection = connection, DetectOpenSearchVersion = false, RetryTimeLimit = TimeSpan.Zero };
            var listener = new RecordingFailureListener();
            using (var sink = new OpenSearchSink(options))
            {
                sink.SetFailureListener(listener);
                sink.Emit(Event());
            }
            await Assert.That(listener.Failures.Any(f => f.Kind == LoggingFailureKind.Permanent)).IsTrue();

            var fallback = new CollectingSink();
            var fallbackConnection = new ScriptedConnection();
            fallbackConnection.EnqueueException(new InvalidOperationException("failed"));
            var fallbackOptions = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { Connection = fallbackConnection, DetectOpenSearchVersion = false, RetryTimeLimit = TimeSpan.Zero };
            using (var logger = new LoggerConfiguration().WriteTo.FallbackChain(
                       first => first.OpenSearch(fallbackOptions), second => second.Sink(fallback)).CreateLogger())
                logger.Information("fallback");
            await Assert.That(fallback.Events).Count().IsEqualTo(1);
        }

        [Test]
        public async Task ValidPartialResponseUsesOnlyLegacyItemHandlingAndContainsHandlerFailure()
        {
            var connection = new ScriptedConnection();
            connection.EnqueueResponse(200, "{\"errors\":true,\"items\":[{\"index\":{\"_index\":\"logs\",\"error\":{\"type\":\"mapping\"}}}]}");
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            {
                Connection = connection, DetectOpenSearchVersion = false,
                FailureSink = new CollectingSink(new InvalidOperationException("handler")),
                EmitEventFailure = EmitEventFailureHandling.WriteToFailureSink
            };
            await new BatchedOpenSearchSink(options).EmitBatchAsync(new[] { Event() });
        }

        private static LogEvent Event() => new(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate("test", Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());
    }
}
