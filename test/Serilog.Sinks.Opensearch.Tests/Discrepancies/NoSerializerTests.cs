using Xunit;

namespace Serilog.Sinks.Opensearch.Tests.Discrepancies
{
    public class NoSerializerTests : OpensearchSinkUniformityTestsBase
    {
        public NoSerializerTests() : base(null) {}

        [Fact]
        public void Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            this.ThrowAndLogAndCatchBulkOutput("test_with_no_serializer");
        }
    }

}
