using System.Threading.Tasks;
using OpenSearch.Net;

namespace Serilog.Sinks.OpenSearch.Tests.Discrepancies
{
    public class OpenSearchDefaultSerializerTests : OpenSearchSinkUniformityTestsBase
    {
        public OpenSearchDefaultSerializerTests() : base(new LowLevelRequestResponseSerializer()) { }

        [Test]
        public async Task Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            await this.ThrowAndLogAndCatchBulkOutput("test_with_default_serializer");
        }
    }

}
