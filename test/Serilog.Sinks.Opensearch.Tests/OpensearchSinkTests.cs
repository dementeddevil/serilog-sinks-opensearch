using OpenSearch.Net;
using Serilog.Sinks.OpenSearch.Tests.Stubs;
using System.Text;
using Xunit;

namespace Serilog.Sinks.OpenSearch.Tests
{
    //public class OpenSearchSinkTests
    //{
    //    [Theory]
    //    [InlineData("8.0.0", "my-logevent", null)]
    //    [InlineData("7.17.5", "my-logevent", null)]
    //    [InlineData("6.8.1", "my-logevent", "my-logevent")]
    //    [InlineData("8.0.0", null, null)]
    //    [InlineData("7.17.5", null, null)]
    //    [InlineData("6.8.1", null, "logevent")]
    //    public void Ctor_DetectOpenSearchVersionSetToTrue_SetsTypeName(string elasticVersion, string configuredTypeName, string expectedTypeName)
    //    {
    //        /* ARRANGE */
    //        var options = new OpenSearchSinkOptions
    //        {
    //            Connection = FakeProductCheckResponse(elasticVersion),
    //            TypeName = configuredTypeName
    //        };

    //        /* ACT */
    //        _ = OpenSearchSinkState.Create(options);

    //        /* Assert */
    //        Assert.Equal(expectedTypeName, options.TypeName);
    //    }

    //    [Theory]
    //    [InlineData("8.0.0", "my-logevent", null)]
    //    [InlineData("7.17.5", "my-logevent", null)]
    //    [InlineData("6.8.1", "my-logevent", null)]
    //    [InlineData("8.0.0", null, null)]
    //    [InlineData("7.17.5", null, null)]
    //    [InlineData("6.8.1", null, null)]
    //    public void Ctor_DetectOpenSearchVersionSetToFalseAssumesVersion7_SetsTypeNameToNull(string elasticVersion, string configuredTypeName, string expectedTypeName)
    //    {
    //        /* ARRANGE */
    //        var options = new OpenSearchSinkOptions
    //        {
    //            Connection = FakeProductCheckResponse(elasticVersion),
    //            DetectOpenSearchVersion = false,
    //            TypeName = configuredTypeName
    //        };

    //        /* ACT */
    //        _ = OpenSearchSinkState.Create(options);

    //        /* Assert */
    //        Assert.Equal(expectedTypeName, options.TypeName);
    //    }

    //    [Theory]
    //    [InlineData("8.0.0", "my-logevent", null)]
    //    [InlineData("7.17.5", "my-logevent", null)]
    //    [InlineData("6.8.1", "my-logevent", "my-logevent")]
    //    [InlineData("8.0.0", null, null)]
    //    [InlineData("7.17.5", null, null)]
    //    [InlineData("6.8.1", null, "logevent")]
    //    public void CreateLogger_DetectOpenSearchVersionSetToTrue_SetsTypeName(string elasticVersion, string configuredTypeName, string expectedTypeName)
    //    {
    //        /* ARRANGE */
    //        var options = new OpenSearchSinkOptions
    //        {
    //            Connection = FakeProductCheckResponse(elasticVersion),
    //            DetectOpenSearchVersion = true,
    //            TypeName = configuredTypeName
    //        };

    //        var loggerConfig = new LoggerConfiguration()
    //            .MinimumLevel.Debug()
    //            .Enrich.WithMachineName()
    //            .WriteTo.Console()
    //            .WriteTo.OpenSearch(options);

    //        /* ACT */
    //        _ = loggerConfig.CreateLogger();

    //        /* Assert */
    //        Assert.Equal(expectedTypeName, options.TypeName);
    //    }

    //    private static IConnection FakeProductCheckResponse(string responseText)
    //    {
    //        var productCheckResponse = ConnectionStub.ModifiedProductCheckResponse(responseText);
    //        return new InMemoryConnection(productCheckResponse);
    //    }
    //}
}
