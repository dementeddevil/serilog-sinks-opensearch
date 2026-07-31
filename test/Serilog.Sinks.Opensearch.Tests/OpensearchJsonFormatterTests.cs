using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Formatting.OpenSearch;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class OpenSearchJsonFormatterTests
    {
        #region Helpers
        static LogEvent CreateLogEvent() =>
        new LogEvent
        (
            DateTimeOffset.Now,
            LogEventLevel.Debug,
           //exception: CreateThrownException(),
            exception: null,
            messageTemplate: new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            properties: Enumerable.Empty<LogEventProperty>()
        );

        static Exception CreateThrownException()
        {
            Exception retEx = null;
            try
            {
                ThrowException();
            }
            catch (Exception ex)
            {
                retEx = ex;
            }
            return retEx;
        }

        static void ThrowException()
        {
            try
            {
                ThrowInnerException();
            }
            catch(Exception ex)
            {
                throw new Exception("Test exception message", ex);
            }
        }

        static void ThrowInnerException()
        {
            throw new Exception("Test inner exception message");
        }

        static Exception CreateThrownExceptionWithNotThrownInner()
        {
            Exception retEx = null;
            try
            {
                throw new Exception("Test exception message", new Exception("Test inner exception message"));
            }
            catch (Exception ex)
            {
                retEx = ex;
            }
            return retEx;
        }

        static LogEvent CreateLogEventWithException(Exception ex) =>
        new LogEvent
        (
            DateTimeOffset.Now,
            LogEventLevel.Debug,
            exception: ex,
            messageTemplate: new MessageTemplate(Enumerable.Empty<MessageTemplateToken>()),
            properties: Enumerable.Empty<LogEventProperty>()
        );

        static async Task CheckProperties(Func<LogEvent> logEventProvider, OpenSearchJsonFormatter formatter, Func<string, Task> assert)
        {
            string result = null;

            var logEvent = logEventProvider();

            using (var stringWriter = new StringWriter())
            {
                formatter.Format(logEvent, stringWriter);

                result = stringWriter.ToString();
            }

            await assert(result);
        }

        static Task CheckProperties(OpenSearchJsonFormatter formatter, Func<string, Task> assert) =>
            CheckProperties(CreateLogEvent, formatter, assert);

        static async Task ContainsProperty(string propertyToCheck, string result) =>
            await Assert.That(result.IndexOf(propertyToCheck, StringComparison.CurrentCultureIgnoreCase)).IsGreaterThanOrEqualTo(0);
        static async Task DoesNotContainsProperty(string propertyToCheck, string result) =>
            await Assert.That(result.IndexOf(propertyToCheck, StringComparison.CurrentCultureIgnoreCase)).IsLessThan(0);
        static string FormatProperty(string property) => $"\"{property}\":";
        #endregion

        [Test]
        [Arguments(OpenSearchJsonFormatter.RenderedMessagePropertyName)]
        [Arguments(OpenSearchJsonFormatter.MessageTemplatePropertyName)]
        [Arguments(OpenSearchJsonFormatter.TimestampPropertyName)]
        [Arguments(OpenSearchJsonFormatter.LevelPropertyName)]
        public async Task DefaultJsonFormater_Should_Render_default_properties(string propertyToCheck)
        {
            await CheckProperties(
                new OpenSearchJsonFormatter(),
                result => ContainsProperty(FormatProperty(propertyToCheck), result));
        }

        [Test]
        public async Task When_disabling_renderMessage_Should_not_render_message_but_others()
        {
            await CheckProperties(
                new OpenSearchJsonFormatter(renderMessage: false),
                async result =>
                {
                    await DoesNotContainsProperty(FormatProperty(OpenSearchJsonFormatter.RenderedMessagePropertyName), result);
                    await ContainsProperty(FormatProperty(OpenSearchJsonFormatter.MessageTemplatePropertyName), result);
                    await ContainsProperty(FormatProperty(OpenSearchJsonFormatter.TimestampPropertyName), result);
                    await ContainsProperty(FormatProperty(OpenSearchJsonFormatter.LevelPropertyName), result);
                });
        }

        [Test]
        public async Task When_disabling_renderMessageTemplate_Should_not_render_message_template_but_others()
        {
            await CheckProperties(
                new OpenSearchJsonFormatter(renderMessageTemplate: false),
                async result =>
                {
                    await DoesNotContainsProperty(FormatProperty(OpenSearchJsonFormatter.MessageTemplatePropertyName), result);
                    await ContainsProperty(FormatProperty(OpenSearchJsonFormatter.RenderedMessagePropertyName), result);
                    await ContainsProperty(FormatProperty(OpenSearchJsonFormatter.TimestampPropertyName), result);
                    await ContainsProperty(FormatProperty(OpenSearchJsonFormatter.LevelPropertyName), result);
                });
        }

        [Test]
        public async Task DefaultJsonFormater_Should_enclose_object()
        {
            await CheckProperties(
                new OpenSearchJsonFormatter(),
                async result =>
                {
                    await Assert.That(result).StartsWith("{");
                    await Assert.That(result).EndsWith($"}}{Environment.NewLine}");
                });
        }

        [Test]
        public async Task When_omitEnclosingObject_should_not_enclose_object()
        {
            await CheckProperties(
                new OpenSearchJsonFormatter(omitEnclosingObject: true),
                async result =>
                {
                    await Assert.That(result).StartsWith("\"");
                    await Assert.That(result).EndsWith("\"");
                });
        }

        [Test]
        public async Task When_provide_closing_delimiter_should_use_it()
        {
            await CheckProperties(
                new OpenSearchJsonFormatter(closingDelimiter: "closingDelimiter"),
                async result =>
                {
                    await Assert.That(result).EndsWith("closingDelimiter");
                });
        }

        [Test]
        public async Task DefaultJsonFormater_Should_Render_Exceptions()
        {
            await CheckProperties(
                () => CreateLogEventWithException(CreateThrownException()),
                new OpenSearchJsonFormatter(),
                async result =>
                {
                    string exceptionsProperty = FormatProperty("exceptions");
                    await ContainsProperty(exceptionsProperty, result);

            
                    string exceptionsValue = result.Substring(result.IndexOf(exceptionsProperty) + exceptionsProperty.Length);

                    // Check the exceptions property is a JSON array
                    await Assert.That(exceptionsValue).StartsWith("[");
                    string stackTraceProperty = FormatProperty("StackTraceString");
                    await ContainsProperty(stackTraceProperty, exceptionsValue);
                    await DoesNotContainsProperty(FormatProperty("StackTrace"), exceptionsValue);

                    string stackTraceValue = exceptionsValue.Substring(exceptionsValue.IndexOf(stackTraceProperty) + stackTraceProperty.Length);

                    // Check the StackTraceString property is a JSON string
                    await Assert.That(stackTraceValue).StartsWith("\"");
                });
        }

        [Test]
        [MethodDataSource(nameof(TestExceptions))]
        public async Task DefaultJsonFormater_Should_Render_Exceptions_With_StackTrace_As_Array(Exception exception)
        {
            await CheckProperties(
                () => CreateLogEventWithException(exception),
                new OpenSearchJsonFormatter(formatStackTraceAsArray: true),
                async result =>
                {
                    string exceptionsProperty = FormatProperty("exceptions");
                    await ContainsProperty(exceptionsProperty, result);


                    string exceptionsValue = result.Substring(result.IndexOf(exceptionsProperty) + exceptionsProperty.Length);

                    // Check the exceptions property is a JSON array
                    await Assert.That(exceptionsValue).StartsWith("[");
                    string stackTraceProperty = FormatProperty("StackTrace");
                    await ContainsProperty(stackTraceProperty, exceptionsValue);
                    await DoesNotContainsProperty(FormatProperty("StackTraceString"), exceptionsValue);

                    string stackTraceValue = exceptionsValue.Substring(exceptionsValue.IndexOf(stackTraceProperty) + stackTraceProperty.Length);

                    // Check the StackTrace property is a JSON array
                    await Assert.That(stackTraceValue).StartsWith("[");
                });
        }

        public static IEnumerable<Func<Exception>> TestExceptions()
        {
            yield return () => CreateThrownException();
            yield return () => CreateThrownExceptionWithNotThrownInner();
            yield return () => new Exception("Not thrown exception message");
        }
    }
}
