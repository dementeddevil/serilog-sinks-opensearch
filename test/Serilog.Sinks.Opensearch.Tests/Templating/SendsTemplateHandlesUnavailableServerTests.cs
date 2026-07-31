using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using Serilog.Debugging;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    [NotInParallel("isolation")]
    public class SendsTemplateHandlesUnavailableServerTests : OpenSearchSinkTestsBase
    {
        [Test]
        public async Task Should_not_crash_when_server_is_unavailable()
        {
            // If this crashes, the test will fail
            CreateLoggerThatCrashes();
        }

        [Test]
        public async Task Should_write_error_to_self_log()
        {
            var selfLogMessages = new StringBuilder();
            SelfLog.Enable(new StringWriter(selfLogMessages));

            // Exception occurs on creation - should be logged
            CreateLoggerThatCrashes();

            var selfLogContents = selfLogMessages.ToString();
            await Assert.That(selfLogContents).Contains("Failed to create the template");
        }

        private static ILogger CreateLoggerThatCrashes()
        {
            var loggerConfig = new LoggerConfiguration()
                .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9199"))
                {
                    AutoRegisterTemplate = true,
                    TemplateName = "crash"
                });

            return loggerConfig.CreateLogger();
        }
    }
}