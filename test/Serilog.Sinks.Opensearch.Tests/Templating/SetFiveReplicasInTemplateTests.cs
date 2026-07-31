using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class SetFiveReplicasInTemplateTests : OpenSearchSinkTestsBase
    {
        private readonly Tuple<Uri, string> _templatePut;

        public SetFiveReplicasInTemplateTests()
        {
            _options.AutoRegisterTemplate = true;
            _options.NumberOfReplicas = 5;

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
        public async Task ShouldRegisterTheCorrectTemplateOnRegistration()
        {
            await JsonEquals(_templatePut.Item2, "template_v1_no-aliases_5replicas.json");
        }

        [Test]
        public async Task TemplatePutToCorrectUrl()
        {
            var uri = _templatePut.Item1;
            await Assert.That(uri.AbsolutePath).IsEqualTo("/_template/serilog-events-template");
        }
    }
}
