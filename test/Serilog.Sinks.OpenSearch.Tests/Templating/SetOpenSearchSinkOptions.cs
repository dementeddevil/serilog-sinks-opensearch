using System;
using FluentAssertions;
using Serilog.Sinks.OpenSearch.Tests.Stubs;
using Xunit;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class SetOpenSearchSinkOptions : OpenSearchSinkTestsBase
    {

        [Fact]
        public void WhenCreatingOptions_NumberOfShardsInjected_NumberOfShardsAreSet()
        {
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9100"))
            {
                AutoRegisterTemplate = true,

                NumberOfShards = 2,
                NumberOfReplicas = 0
            };
                       
            options.NumberOfReplicas.Should().Be(0);
            options.NumberOfShards.Should().Be(2);

        }
    }
}