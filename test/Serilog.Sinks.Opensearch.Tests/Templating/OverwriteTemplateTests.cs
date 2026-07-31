using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class OverwriteTemplateTests : OpenSearchSinkTestsBase
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
                .WriteTo.OpenSearch(_options);

            var logger = loggerConfig.CreateLogger();
            using (logger as IDisposable)
            {
                logger.Error("Test exception. Should not contain an embedded exception object.");
            }
        }

        [Test]
        public async Task ShouldOverwriteTemplate()
        {
            DoRegister();
            await Assert.That(this._seenHttpPosts).IsNotNull().And.IsNotEmpty().And.Count().IsEqualTo(1);
            await Assert.That(this._seenHttpHeads == null || this._seenHttpHeads.Count == 0).IsTrue();
            await Assert.That(this._seenHttpPuts).IsNotNull().And.IsNotEmpty().And.Count().IsEqualTo(1);
        }


    }
}
