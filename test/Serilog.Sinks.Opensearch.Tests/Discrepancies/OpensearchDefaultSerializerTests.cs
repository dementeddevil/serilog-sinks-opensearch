using OpenSearch.Net;
using Xunit;

namespace Serilog.Sinks.Opensearch.Tests.Discrepancies
{
    public class OpensearchDefaultSerializerTests : OpensearchSinkUniformityTestsBase
    {
        public OpensearchDefaultSerializerTests() : base(new LowLevelRequestResponseSerializer()) { }

        [Fact]
        public void Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            this.ThrowAndLogAndCatchBulkOutput("test_with_default_serializer");
        }
    }

}
