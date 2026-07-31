using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class Sendsv1TemplateTests : OpenSearchSinkTestsBase
    {
        private readonly Tuple<Uri, string> _templatePut;

        public Sendsv1TemplateTests()
        {
            _options.AutoRegisterTemplate = true;
            _options.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1;
            _options.IndexAliases = new string[] { "logstash" };

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
        }

        [Test]
        public async Task ShouldRegisterTheVersion1TemplateOnRegistrationWhenDetectedOpenSearchVersionIsV1()
        {
            await JsonEquals(_templatePut.Item2, "template_v1.json");
        }

        [Test]
        public async Task TemplatePutToCorrectUrl()
        {
            var uri = _templatePut.Item1;
            await Assert.That(uri.AbsolutePath).IsEqualTo("/_template/serilog-events-template");
        }
    }
}
