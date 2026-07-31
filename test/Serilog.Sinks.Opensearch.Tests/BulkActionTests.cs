using System.Threading.Tasks;
using System;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class BulkActionTests : OpenSearchSinkTestsBase
    {
        [Test]
        [Skip("Flaky test on GitHub actions")]
        public async Task DefaultBulkActionV7()
        {
            _options.IndexFormat = "logs";
            _options.TypeName = "_doc";
            _options.PipelineName = null;
            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_type"":""_doc"",""_index"":""logs""}}";
            await Assert.That(bulkJsonPieces[0]).IsEqualTo(expectedAction);
        }

        [Test]

        [Skip("Flaky test on GitHub actions")]
        public async Task BulkActionV7OverrideTypeName()
        {
            _options.IndexFormat = "logs";
            _options.TypeName = null; // This is the default value, starting v9.0.0
            _options.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1;
            _options.PipelineName = null;
            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_type"":""_doc"",""_index"":""logs""}}";
            await Assert.That(bulkJsonPieces[0]).IsEqualTo(expectedAction);
        }

        [Test]

        [Skip("Flaky test on GitHub actions")]
        public async Task DefaultBulkActionV8()
        {
            _options.IndexFormat = "logs";
            _options.TypeName = null;
            _options.PipelineName = null;
            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_index"":""logs""}}";
            await Assert.That(bulkJsonPieces[0]).IsEqualTo(expectedAction);
        }


        [Test]


        [Skip("Flaky test on GitHub actions")]
        public async Task BulkActionDataStreams()
        {
            _options.IndexFormat = "logs-my-stream";
            _options.TypeName = null;
            _options.PipelineName = null;
            _options.BatchAction = OpenOpType.Create;
            
            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""create"":{""_index"":""logs-my-stream""}}";
            await Assert.That(bulkJsonPieces[0]).IsEqualTo(expectedAction);
        }

        [Test]

        [Skip("Flaky test on GitHub actions")]
        public async Task PipelineAction()
        {
            _options.IndexFormat = "logs-my-stream";
            _options.TypeName = "_doc";
            _options.PipelineName = "my-pipeline";
            _options.BatchAction = OpenOpType.Index;
            
            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_type"":""_doc"",""_index"":""logs-my-stream"",""pipeline"":""my-pipeline""}}";
            await Assert.That(bulkJsonPieces[0]).IsEqualTo(expectedAction);
        }

        private static LogEvent ADummyLogEvent() {
            return new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
                new MessageTemplate("A template", Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());
        }
    }
}