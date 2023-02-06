using System;
using System.Linq;
using FluentAssertions;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.Opensearch.Tests.Stubs;
using Xunit;

namespace Serilog.Sinks.Opensearch.Tests
{
    public class BulkActionTests : OpensearchSinkTestsBase
    {
        [Fact(Skip = "Flaky test on GitHub actions")]
        public void DefaultBulkActionV7()
        {
            _options.IndexFormat = "logs";
            _options.TypeName = "_doc";
            _options.PipelineName = null;
            using (var sink = new OpensearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_type"":""_doc"",""_index"":""logs""}}";
            bulkJsonPieces[0].Should().Be(expectedAction);
        }

        [Fact(Skip = "Flaky test on GitHub actions")]
        public void BulkActionV7OverrideTypeName()
        {
            _options.IndexFormat = "logs";
            _options.TypeName = null; // This is the default value, starting v9.0.0
            _options.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1;
            _options.PipelineName = null;
            using (var sink = new OpensearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_type"":""_doc"",""_index"":""logs""}}";
            bulkJsonPieces[0].Should().Be(expectedAction);
        }

        [Fact(Skip = "Flaky test on GitHub actions")]
        public void DefaultBulkActionV8()
        {
            _options.IndexFormat = "logs";
            _options.TypeName = null;
            _options.PipelineName = null;
            using (var sink = new OpensearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_index"":""logs""}}";
            bulkJsonPieces[0].Should().Be(expectedAction);
        }


        [Fact(Skip = "Flaky test on GitHub actions")]
        public void BulkActionDataStreams()
        {
            _options.IndexFormat = "logs-my-stream";
            _options.TypeName = null;
            _options.PipelineName = null;
            _options.BatchAction = OpenOpType.Create;
            
            using (var sink = new OpensearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""create"":{""_index"":""logs-my-stream""}}";
            bulkJsonPieces[0].Should().Be(expectedAction);
        }

        [Fact(Skip = "Flaky test on GitHub actions")]
        public void PipelineAction()
        {
            _options.IndexFormat = "logs-my-stream";
            _options.TypeName = "_doc";
            _options.PipelineName = "my-pipeline";
            _options.BatchAction = OpenOpType.Index;
            
            using (var sink = new OpensearchSink(_options))
            {
                sink.Emit(ADummyLogEvent());
                sink.Emit(ADummyLogEvent());
            }

            var bulkJsonPieces = this.AssertSeenHttpPosts(_seenHttpPosts, 2, 1);
            const string expectedAction = @"{""index"":{""_type"":""_doc"",""_index"":""logs-my-stream"",""pipeline"":""my-pipeline""}}";
            bulkJsonPieces[0].Should().Be(expectedAction);
        }

        private static LogEvent ADummyLogEvent() {
            return new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
                new MessageTemplate("A template", Enumerable.Empty<MessageTemplateToken>()),
                Enumerable.Empty<LogEventProperty>());
        }
    }
}