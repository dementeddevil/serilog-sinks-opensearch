using System;
using System.Threading.Tasks;
using Serilog.Configuration;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class NativeBatchingOptionsTests
    {
        [Test]
        public async Task MapsExistingBatchingOptionsAndRetryLimit()
        {
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            {
                BatchPostingLimit = 17,
                Period = TimeSpan.FromSeconds(3),
                QueueSizeLimit = -1,
                RetryTimeLimit = TimeSpan.Zero
            };

            BatchingOptions mapped = OpenSearchSink.CreateBatchingOptions(options);
            await Assert.That(mapped.BatchSizeLimit).IsEqualTo(17);
            await Assert.That(mapped.BufferingTimeLimit).IsEqualTo(TimeSpan.FromSeconds(3));
            await Assert.That(mapped.EagerlyEmitFirstEvent).IsTrue();
            await Assert.That(mapped.QueueLimit).IsNull();
            await Assert.That(mapped.RetryTimeLimit).IsEqualTo(TimeSpan.Zero);
        }

        [Test]
        [Arguments(0)]
        [Arguments(-2)]
        public async Task RejectsInvalidQueueLimits(int value)
        {
            Exception failure = null;
            try { new OpenSearchSinkOptions(new Uri("http://localhost:9200")) { QueueSizeLimit = value }; }
            catch (Exception exception) { failure = exception; }
            await Assert.That(failure).IsTypeOf<ArgumentOutOfRangeException>();
        }

        [Test]
        public async Task NullRetryRetainsNativeDefault()
        {
            var mapped = OpenSearchSink.CreateBatchingOptions(new OpenSearchSinkOptions(new Uri("http://localhost:9200")));
            await Assert.That(mapped.RetryTimeLimit).IsEqualTo(TimeSpan.FromMinutes(10));
        }

        [Test]
        public async Task MapsPositiveQueueAndRetryLimits()
        {
            var options = new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
            { QueueSizeLimit = 42, RetryTimeLimit = TimeSpan.FromSeconds(9) };
            var mapped = OpenSearchSink.CreateBatchingOptions(options);
            await Assert.That(mapped.QueueLimit).IsEqualTo(42);
            await Assert.That(mapped.RetryTimeLimit).IsEqualTo(TimeSpan.FromSeconds(9));
        }

        [Test]
        public async Task NegativeRetryFailsDuringNativeSinkConstruction()
        {
            Exception failure = null;
            try
            {
                using var sink = new OpenSearchSink(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
                { RetryTimeLimit = TimeSpan.FromSeconds(-1), DetectOpenSearchVersion = false });
            }
            catch (Exception exception) { failure = exception; }
            await Assert.That(failure).IsTypeOf<ArgumentOutOfRangeException>();
        }
    }
}
