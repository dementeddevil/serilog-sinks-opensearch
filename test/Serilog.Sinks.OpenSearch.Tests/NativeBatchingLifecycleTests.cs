using System;
using System.Threading.Tasks;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class NativeBatchingLifecycleTests
    {
        [Test]
        public async Task DisposalFlushesAcceptedEvent()
        {
            var connection = new ScriptedConnection();
            using (var logger = new LoggerConfiguration().WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { Connection = connection, DetectOpenSearchVersion = false }).CreateLogger())
                logger.Information("flush me");
            await connection.WaitForRequestAsync(TimeSpan.FromSeconds(10));
            await Assert.That(connection.RequestBodies).Count().IsEqualTo(1);
        }
    }
}
