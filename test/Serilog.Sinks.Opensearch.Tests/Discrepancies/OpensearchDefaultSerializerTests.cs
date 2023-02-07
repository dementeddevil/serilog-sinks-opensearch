using OpenSearch.Net;
using Xunit;

namespace Serilog.Sinks.OpenSearch.Tests.Discrepancies
{
    public class OpenSearchDefaultSerializerTests : OpenSearchSinkUniformityTestsBase
    {
        public OpenSearchDefaultSerializerTests() : base(new LowLevelRequestResponseSerializer()) { }

        [Fact]
        public void Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            this.ThrowAndLogAndCatchBulkOutput("test_with_default_serializer");
        }
    }

}
