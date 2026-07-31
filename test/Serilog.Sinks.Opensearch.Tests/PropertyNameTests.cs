using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class PropertyNameTests : OpenSearchSinkTestsBase
    {

        [Test]
        public async Task UsesCustomPropertyNames()
        {
            try
            {
                await this.ThrowAsync();
            }
            catch (Exception e)
            {
                var timestamp = new DateTimeOffset(2013, 05, 28, 22, 10, 20, 666, TimeSpan.FromHours(10));
                var messageTemplate = "{Song}++";
                var template = new MessageTemplateParser().Parse(messageTemplate);
                using (var sink = new OpenSearchSink(_options))
                {
                    var properties = new List<LogEventProperty>
                    {
                        new LogEventProperty("Song", new ScalarValue("New Macabre")), 
                        new LogEventProperty("Complex", new ScalarValue(new { A = 1, B = 2 }))
                    };
                    var logEvent = new LogEvent(timestamp, LogEventLevel.Information, e, template, properties);
                    //one off
                    sink.Emit(logEvent);

                    sink.Emit(logEvent);
                    logEvent = new LogEvent(timestamp.AddDays(2), LogEventLevel.Information, e, template, properties);
                    sink.Emit(logEvent);
                }
                var bulkJsonPieces = await this.AssertSeenHttpPosts(_seenHttpPosts, 4);
                await Assert.That(bulkJsonPieces[0]).Contains(@"""_index"":""logstash-2013.05.28");
                await Assert.That(bulkJsonPieces[1]).Contains("New Macabre");
                await Assert.That(bulkJsonPieces[1]).DoesNotContain("Properties\"");
                await Assert.That(bulkJsonPieces[1]).Contains("fields\":{");
                await Assert.That(bulkJsonPieces[1]).Contains("@timestamp");
                await Assert.That(bulkJsonPieces[2]).Contains(@"""_index"":""logstash-2013.05.30");
            }
        }
    }

}
