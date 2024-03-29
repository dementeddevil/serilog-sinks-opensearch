﻿using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;
using Serilog.Debugging;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Templating
{
    [Collection("isolation")]
    public class DiscoverVersionHandlesUnavailableServerTests : OpenSearchSinkTestsBase
    {
        [Fact]
        public void Should_not_crash_when_server_is_unavailable()
        {
            // If this crashes, the test will fail
            CreateLoggerThatCrashes();
        }

        [Fact]
        public void Should_write_error_to_self_log()
        {
            var selfLogMessages = new StringBuilder();
            SelfLog.Enable(new StringWriter(selfLogMessages));

            // Exception occurs on creation - should be logged
            var logger = CreateLoggerThatCrashes();
            logger.Information("This is a test");

            var selfLogContents = selfLogMessages.ToString();
            selfLogContents.Should().Contain("Failed to discover the cluster version");
        }

        private static ILogger CreateLoggerThatCrashes()
        {
            var loggerConfig = new LoggerConfiguration()
                .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9199"))
                {
                    DetectOpenSearchVersion = true,
                    AutoRegisterTemplate = true,
                });

            return loggerConfig.CreateLogger();
        }
    }
}