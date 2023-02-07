using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;
using Xunit;

namespace Serilog.Sinks.OpenSearch.Tests.Discrepancies
{
    public class JsonNetSerializerTests : OpenSearchSinkUniformityTestsBase
    {
        public JsonNetSerializerTests() : base(JsonNetSerializer.Default(LowLevelRequestResponseSerializer.Instance, new ConnectionSettings())) { }

        [Fact]
        public void Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            this.ThrowAndLogAndCatchBulkOutput("test_with_jsonnet_serializer");
        }
    }

}
