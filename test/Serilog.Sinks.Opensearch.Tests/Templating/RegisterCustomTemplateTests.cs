using System.Threading.Tasks;
using System;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    public class RegisterCustomTemplateTests : OpenSearchSinkTestsBase
    {
        private const string CustomTemplateContent = @"{ template: ""my-custom-template-*"" }";
        private readonly Tuple<Uri, string> _templatePut;

        public RegisterCustomTemplateTests()
        {
            _options.AutoRegisterTemplate = true;
            _options.GetTemplateContent = () => CustomTemplateContent;
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
        public async Task ShouldRegisterCustomTemplate()
        {
            await Assert.That(this._templatePut.Item2).IsEqualTo(CustomTemplateContent).IgnoringCase();
        }
    }
}
