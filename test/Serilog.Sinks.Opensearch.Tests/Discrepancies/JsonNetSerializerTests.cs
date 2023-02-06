using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;
using Xunit;

namespace Serilog.Sinks.Opensearch.Tests.Discrepancies
{
    public class JsonNetSerializerTests : OpensearchSinkUniformityTestsBase
    {
        public JsonNetSerializerTests() : base(JsonNetSerializer.Default(LowLevelRequestResponseSerializer.Instance, new ConnectionSettings())) { }

        [Fact]
        public void Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            this.ThrowAndLogAndCatchBulkOutput("test_with_jsonnet_serializer");
        }
    }

}
