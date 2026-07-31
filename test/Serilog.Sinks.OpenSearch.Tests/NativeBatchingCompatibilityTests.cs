using System;
using System.Threading.Tasks;
using Serilog.Core;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class NativeBatchingCompatibilityTests
    {
        [Test]
        public async Task PublicSinkPreservesSourceContracts()
        {
            using var sink = new OpenSearchSink(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { DetectOpenSearchVersion = false });
            await Assert.That(sink).IsAssignableTo<ILogEventSink>();
            await Assert.That(sink).IsAssignableTo<IDisposable>();
            await Assert.That(sink).IsAssignableTo<ISetLoggingFailureListener>();
        }
    }
}
