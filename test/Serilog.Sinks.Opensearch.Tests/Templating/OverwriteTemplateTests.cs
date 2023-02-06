using System;
using FluentAssertions;
using Serilog.Sinks.Opensearch.Tests.Stubs;
using Xunit;

namespace Serilog.Sinks.Opensearch.Tests.Templating
{
    public class OverwriteTemplateTests : OpensearchSinkTestsBase
    {
        private void DoRegister()
        {
            _templateExistsReturnCode = 200;

            _options.AutoRegisterTemplate = true;
            _options.OverwriteTemplate = true;
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.Opensearch(_options);

            var logger = loggerConfig.CreateLogger();
            using (logger as IDisposable)
            {
                logger.Error("Test exception. Should not contain an embedded exception object.");
            }
        }

        [Fact]
        public void ShouldOverwriteTemplate()
        {
            DoRegister();
            this._seenHttpPosts.Should().NotBeNullOrEmpty().And.HaveCount(1);
            this._seenHttpHeads.Should().BeNullOrEmpty();
            this._seenHttpPuts.Should().NotBeNullOrEmpty().And.HaveCount(1);
        }


    }
}