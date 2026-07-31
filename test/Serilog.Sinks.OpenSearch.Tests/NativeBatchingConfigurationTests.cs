using System;
using System.Threading.Tasks;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class NativeBatchingConfigurationTests
    {
        [Test]
        public async Task EagerlyEmitsFirstEventBeforeLongPeriod()
        {
            var connection = new ScriptedConnection();
            using var logger = new LoggerConfiguration().WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { Connection = connection, DetectOpenSearchVersion = false, Period = TimeSpan.FromMinutes(1) }).CreateLogger();
            logger.Information("eager");
            await connection.WaitForRequestAsync(TimeSpan.FromSeconds(10));
            await Assert.That(connection.RequestBodies).Count().IsEqualTo(1);
        }
    }
}
