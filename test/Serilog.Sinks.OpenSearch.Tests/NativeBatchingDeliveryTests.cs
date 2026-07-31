using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class NativeBatchingDeliveryTests
    {
        [Test]
        public async Task PropagatesTransportExceptionsRegardlessOfLegacyHandling()
        {
            var expected = new InvalidOperationException("transport failed");
            var connection = new ScriptedConnection();
            connection.EnqueueException(expected);
            var options = Options(connection);
            options.EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog;
            var sink = new BatchedOpenSearchSink(options);

            Exception failure = null;
            try { await sink.EmitBatchAsync(new[] { Event() }); }
            catch (Exception exception) { failure = exception; }

            await Assert.That(failure).IsNotNull();
            await Assert.That(failure.InnerException).IsSameReferenceAs(expected);
        }

        [Test]
        public async Task UnsuccessfulResponseWithoutOriginalExceptionThrowsSafeDiagnostic()
        {
            var connection = new ScriptedConnection();
            connection.EnqueueResponse(503, "{\"error\":\"unavailable\"}");
            var sink = new BatchedOpenSearchSink(Options(connection));
            Exception failure = null;
            try { await sink.EmitBatchAsync(new[] { Event() }); }
            catch (Exception exception) { failure = exception; }
            await Assert.That(failure).IsNotNull();
        }

        [Test]
        public async Task EmptyBatchCallbackCompletes() =>
            await new BatchedOpenSearchSink(Options(new ScriptedConnection())).OnEmptyBatchAsync();

        [Test]
        public async Task NativeBatchingRetriesTheExactBatchThenRecovers()
        {
            var connection = new ScriptedConnection();
            connection.EnqueueException(new InvalidOperationException("temporary"));
            connection.EnqueueResponse();
            var options = Options(connection);
            options.RetryTimeLimit = TimeSpan.FromSeconds(12);

            using (var logger = new LoggerConfiguration().WriteTo.OpenSearch(options).CreateLogger())
            {
                logger.Information("retry me");
                await connection.WaitForRequestAsync(TimeSpan.FromSeconds(10));
                await connection.WaitForRequestAsync(TimeSpan.FromSeconds(15));
            }

            var requests = connection.RequestBodies.ToArray();
            await Assert.That(requests).Count().IsEqualTo(2);
            await Assert.That(requests[1]).IsEqualTo(requests[0]);
        }

        private static OpenSearchSinkOptions Options(ScriptedConnection connection) =>
            new(new Uri("http://localhost:9200")) { Connection = connection, DetectOpenSearchVersion = false };

        private static LogEvent Event() => new(DateTimeOffset.Now, LogEventLevel.Information, null,
            new MessageTemplate("test", Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());
    }
}
