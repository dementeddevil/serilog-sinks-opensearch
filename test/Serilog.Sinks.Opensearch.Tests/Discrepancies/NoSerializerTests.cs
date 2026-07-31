using System.Threading.Tasks;

namespace Serilog.Sinks.OpenSearch.Tests.Discrepancies
{
    public class NoSerializerTests : OpenSearchSinkUniformityTestsBase
    {
        public NoSerializerTests() : base(null) {}

        [Test]
        public async Task Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            await this.ThrowAndLogAndCatchBulkOutput("test_with_no_serializer");
        }
    }

}
