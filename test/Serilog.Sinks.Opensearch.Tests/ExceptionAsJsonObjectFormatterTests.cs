using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;
using Serilog.Formatting.OpenSearch;
using Serilog.Parsing;
using Serilog.Sinks.OpenSearch.Tests.Domain;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class ExceptionAsJsonObjectFormatterTests : OpenSearchSinkTestsBase
    {
        private static readonly MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();

        public ExceptionAsJsonObjectFormatterTests() : base()
        { 

            _options.CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage:true);
        }

        [Test]
        public async Task WhenLoggingAnEvent_OutputsValidJson()
        {
            const string expectedMessage = "test";

            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(LogEventWithMessage(expectedMessage));
            }

            var eventWritten = (await AssertAndGetJsonEvents()).First();
            await Assert.That(eventWritten.Level).IsEqualTo(LogEventLevel.Warning);
            await Assert.That(eventWritten.Message).IsEqualTo(expectedMessage);
        }

        [Test]
        public async Task WhenLogging_WithException_ExceptionShouldBeRenderedInExceptionField()
        {
            const string expectedExceptionMessage = "test exception";

            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(LogEventWithMessage("test", new Exception(expectedExceptionMessage)));
            }

            var eventWritten = (await AssertAndGetJsonEvents()).First();
            var exceptionInfo = eventWritten.Exception;
            await Assert.That(exceptionInfo).IsNotNull();
            await Assert.That(exceptionInfo.Message).IsEqualTo(expectedExceptionMessage);
#if NETFRAMEWORK
            await Assert.That(exceptionInfo.ClassName).IsEqualTo("System.Exception");
#endif
        }

        [Test]
        public async Task WhenLogging_ExceptionWithInner_ExceptionShouldIncludeInnerExceptions()
        {
            var inner = new InvalidOperationException();
            var exception = new Exception("outer", inner);

            using (var sink = new OpenSearchSink(_options))
            {
                sink.Emit(LogEventWithMessage("test", exception));
            }

            var eventWritten = (await AssertAndGetJsonEvents()).First();
            var exceptionInfo = eventWritten.Exception;
            await Assert.That(exceptionInfo.InnerException).IsNotNull();
        }

        private static LogEvent LogEventWithMessage(string expectedMessage, Exception exception = null)
        {
            var template = _messageTemplateParser.Parse(expectedMessage);
            return new LogEvent(DateTimeOffset.Now, LogEventLevel.Warning, exception, template, Enumerable.Empty<LogEventProperty>());
        }

        private async Task<IEnumerable<KibanaFriendlyJsonEvent>> AssertAndGetJsonEvents()
        {
            await Assert.That(_seenHttpPosts).IsNotEmpty();
            return _seenHttpPosts.SelectMany(postedData => postedData.Split(new char[] { '\n'}, StringSplitOptions.RemoveEmptyEntries))
                .Where((_,i) => i % 2 == 1)
                .Select(JsonConvert.DeserializeObject<KibanaFriendlyJsonEvent>);
        }


        class KibanaFriendlyJsonEvent : IBulkData
        {
            [JsonProperty("@timestamp")]
            public DateTime Timestamp { get; set; }

            [JsonProperty("level")]
            [JsonConverter(typeof(StringEnumConverter))]
            public LogEventLevel Level { get; set; }

            [JsonProperty("messageTemplate")]
            public string MessageTemplate { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("exception")]
            public SerilogOpenSearchExceptionInfoWithInner Exception { get; set; }
        }

        class SerilogOpenSearchExceptionInfoWithInner : SerilogOpenSearchExceptionInfo
        {
            [JsonProperty("innerException")]
            public SerilogOpenSearchExceptionInfo InnerException { get; set; }
        }
    }


}
