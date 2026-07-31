using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;

namespace Serilog.Sinks.OpenSearch.Tests.Discrepancies
{
    public class JsonNetSerializerTests : OpenSearchSinkUniformityTestsBase
    {
        public JsonNetSerializerTests() : base(JsonNetSerializer.Default(LowLevelRequestResponseSerializer.Instance, new ConnectionSettings())) { }

        [Test]
        public async Task Should_SerializeToExpandedExceptionObjectWhenExceptionIsSet()
        {
            await this.ThrowAndLogAndCatchBulkOutput("test_with_jsonnet_serializer");
        }
    }

}
