using System;
using FluentAssertions;
using Serilog.Sinks.Opensearch.Tests.Stubs;
using Xunit;

namespace Serilog.Sinks.Opensearch.Tests.Templating
{
    public class SetOpensearchSinkOptions : OpensearchSinkTestsBase
    {

        [Fact]
        public void WhenCreatingOptions_NumberOfShardsInjected_NumberOfShardsAreSet()
        {
            var options = new OpensearchSinkOptions(new Uri("http://localhost:9100"))
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