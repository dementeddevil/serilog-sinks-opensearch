using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class SetOpenSearchSinkOptions : OpenSearchSinkTestsBase
    {

        [Test]
        public async Task WhenCreatingOptions_NumberOfShardsInjected_NumberOfShardsAreSet()
        {
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9100"))
            {
                AutoRegisterTemplate = true,

                NumberOfShards = 2,
                NumberOfReplicas = 0
            };
                       
            await Assert.That(options.NumberOfReplicas).IsEqualTo(0);
            await Assert.That(options.NumberOfShards).IsEqualTo(2);
        }
    }
}