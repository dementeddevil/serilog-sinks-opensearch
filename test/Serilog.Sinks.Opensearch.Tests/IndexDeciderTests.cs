using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class IndexDeciderTests : OpenSearchSinkTestsBase
    {
        [Test]
        public async Task IndexDecider_EndsUpInTheOutput()
        {
            //DO NOTE that you cant send objects as scalar values through Logger.*("{Scalar}", {});
            var timestamp = new DateTimeOffset(2013, 05, 28, 22, 10, 20, 666, TimeSpan.FromHours(10));
            const string messageTemplate = "{Song}++ @{Complex}";
            var template = new MessageTemplateParser().Parse(messageTemplate);
            _options.IndexDecider = (l, utcTime) => string.Format("logstash-{1}-{0:yyyy.MM.dd}", utcTime, l.Level.ToString().ToLowerInvariant());
            using (var sink = new OpenSearchSink(_options))
            {
                var properties = new List<LogEventProperty> { new LogEventProperty("Song", new ScalarValue("New Macabre")) };
                var e = new LogEvent(timestamp, LogEventLevel.Information, null, template, properties);
                //one off 
                sink.Emit(e);


                sink.Emit(e);
                var exception = new ArgumentException("parameter");
                properties = new List<LogEventProperty>
                {
                    new LogEventProperty("Song", new ScalarValue("Old Macabre")),
                    new LogEventProperty("Complex", new ScalarValue(new { A  = 1, B = 2}))
                };
                e = new LogEvent(timestamp.AddYears(-2), LogEventLevel.Fatal, exception, template, properties);
                sink.Emit(e);
            }

            var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 4);
            await Assert.That(bulkJsonPieces[0]).Contains(@"""_index"":""logstash-information-2013.05.28");
            await Assert.That(bulkJsonPieces[1]).Contains("New Macabre");
            await Assert.That(bulkJsonPieces[2]).Contains(@"""_index"":""logstash-fatal-2011.05.28");
            await Assert.That(bulkJsonPieces[3]).Contains("Old Macabre");
            //serilog by default simpy .ToString()'s unknown objects
            await Assert.That(bulkJsonPieces[3]).Contains("Complex\":{");
        }
    }
}