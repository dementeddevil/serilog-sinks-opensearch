using System.Threading.Tasks;
using System;
using System.Runtime.Serialization;
using OpenSearch.Net;
using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests.Discrepancies
{
    public class OpenSearchSinkUniformityTestsBase : OpenSearchSinkTestsBase
    {
        public OpenSearchSinkUniformityTestsBase(IOpenSearchSerializer serializer)
        {
            _options.Serializer = serializer;
        }

        public async Task ThrowAndLogAndCatchBulkOutput(string exceptionMessage)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.OpenSearch(_options);

            var logger = loggerConfig.CreateLogger();
            using (logger as IDisposable)
            {
                try
                {
                    try
                    {
                        throw new Exception("inner most exception");
                    }
                    catch (Exception e)
                    {
                        var innerException = new NastyException("nasty inner exception", e);
                        throw new Exception(exceptionMessage, innerException);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Test exception. Should contain an embedded exception object.");
                }
                logger.Error("Test exception. Should not contain an embedded exception object.");
            }

            var postedEvents = await this.GetPostedLogEvents(expectedCount: 2);
            Console.WriteLine("BULK OUTPUT BEGIN ==========");
            foreach (var post in _seenHttpPosts)
                Console.WriteLine(post);
            Console.WriteLine("BULK OUTPUT END ============");

            var firstEvent = postedEvents[0];
            await Assert.That(firstEvent.Exceptions).IsNotNull().And.Count().IsEqualTo(3);
            await Assert.That(firstEvent.Exceptions[0].Message).IsNotNullOrWhiteSpace().And.IsEqualTo(exceptionMessage);
            var realException = firstEvent.Exceptions[0];
#if !NO_SERIALIZATION
#if NETFRAMEWORK
            await Assert.That(realException.ExceptionMethod).IsNotNull();
            await Assert.That(realException.ExceptionMethod.Name).IsNotNullOrWhiteSpace();
            await Assert.That(realException.ExceptionMethod.AssemblyName).IsNotNullOrWhiteSpace();
            await Assert.That(realException.ExceptionMethod.AssemblyVersion).IsNotNullOrWhiteSpace();
            await Assert.That(realException.ExceptionMethod.ClassName).IsNotNullOrWhiteSpace();
            await Assert.That(realException.ExceptionMethod.Signature).IsNotNullOrWhiteSpace();
            await Assert.That(realException.ExceptionMethod.MemberType).IsGreaterThan(0);
#endif
            var nastyException = firstEvent.Exceptions[1];
            await Assert.That(nastyException.Depth).IsEqualTo(1);
            await Assert.That(nastyException.Message).IsEqualTo("nasty inner exception");
            await Assert.That(nastyException.StackTraceString).IsEqualTo("stack trace string");
            await Assert.That(nastyException.HelpUrl).IsEqualTo("help url");
            await Assert.That(nastyException.RemoteStackTraceString).IsEqualTo("remote stack trace string");
            await Assert.That(nastyException.RemoteStackIndex).IsEqualTo(1);
            await Assert.That(nastyException.ClassName).IsEqualTo("classname nasty exception");
            await Assert.That(nastyException.HResult).IsEqualTo(123123);
            await Assert.That(nastyException.Source).IsEqualTo("source");
#endif
            // await Assert.That(nastyException.WatsonBuckets).IsEquivalentTo(new byte[] { 1, 2, 3 });
            var secondEvent = postedEvents[1];
            await Assert.That(secondEvent.Exceptions == null || secondEvent.Exceptions.Count == 0).IsTrue();
        }
    }

    /// <summary>
    /// Exception that forces often empty serializationinfo values to have a value
    /// </summary>
    public class NastyException : Exception
    {
        public NastyException(string message) : base(message) { }
        public NastyException(string message, Exception innerException) : base(message, innerException) { }

#if !NO_SERIALIZATION
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Message", this.Message);
            info.AddValue("HelpURL", "help url");
            info.AddValue("StackTraceString", "stack trace string");

            info.AddValue("RemoteStackTraceString", "remote stack trace string");
            info.AddValue("RemoteStackIndex", 1);
            info.AddValue("ExceptionMethod", "exception method");
            info.AddValue("HResult", 123123);
            info.AddValue("Source", "source");
            info.AddValue("ClassName", "classname nasty exception");
            info.AddValue("WatsonBuckets", new byte[] { 1, 2, 3 }, typeof(byte[]));
        }
#endif
    }
}
