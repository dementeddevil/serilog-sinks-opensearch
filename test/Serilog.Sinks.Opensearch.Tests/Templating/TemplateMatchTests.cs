using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class TemplateMatchTests : OpenSearchSinkTestsBase
    {
        private readonly Tuple<Uri, string> _templatePut;

        public TemplateMatchTests()
        {
            _options.AutoRegisterTemplate = true;
            _options.IndexFormat = "dailyindex-{0:yyyy.MM.dd}-mycompany";
            _options.TemplateName = "dailyindex-logs-template";
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

            _templatePut = this._seenHttpPuts[0];
            
        }

        [Before(Test)]
        public async Task AssertRegistration()
        {
            await Assert.That(this._seenHttpPosts).IsNotNull().And.IsNotEmpty().And.Count().IsEqualTo(1);
            await Assert.That(this._seenHttpPuts).IsNotNull().And.IsNotEmpty().And.Count().IsEqualTo(1);
            await Assert.That(this._seenHttpHeads).IsNotNull().And.IsNotEmpty().And.Count().IsEqualTo(1);
        }

        [Test]
        public async Task TemplatePutToCorrectUrl()
        {
            var uri = this._templatePut.Item1;
            await Assert.That(uri.AbsolutePath).IsEqualTo("/_template/dailyindex-logs-template");
        }

        [Test]
        public async Task TemplateMatchShouldReflectConfiguredIndexFormat()
        {
            var json = this._templatePut.Item2;
            await Assert.That(json).Contains(@"""index_patterns"":[""dailyindex-*-mycompany""]");
        }

    }
}
