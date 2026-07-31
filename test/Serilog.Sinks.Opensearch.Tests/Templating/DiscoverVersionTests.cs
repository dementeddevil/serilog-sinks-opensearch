using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class DiscoverVersionTests : OpenSearchSinkTestsBase
    {
        private readonly Tuple<Uri,int> _templateGet;

        public DiscoverVersionTests()
        {
            _options.DetectOpenSearchVersion = true;

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.OpenSearch(_options);

            var logger = loggerConfig.CreateLogger();
            using ((IDisposable) logger)
            {
                logger.Error("Test exception. Should not contain an embedded exception object.");
            }

            _templateGet = this._seenHttpGets[0];
        }

        [Before(Test)]
        public async Task AssertRegistration()
        {
            await Assert.That(this._seenHttpGets).IsNotNull().And.IsNotEmpty().And.Count().IsEqualTo(1);
        }

       
        [Test]
        public async Task TemplatePutToCorrectUrl()
        {
            var uri = _templateGet.Item1;
            await Assert.That(uri.AbsolutePath).IsEqualTo("/");
        }
    }
}
