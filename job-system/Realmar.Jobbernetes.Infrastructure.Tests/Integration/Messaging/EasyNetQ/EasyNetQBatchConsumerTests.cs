using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Integration.Messaging.EasyNetQ
{
    public class EasyNetQBatchConsumerTests : EasyNetQBaseTest
    {
        [Fact]
        public async Task ReachBatchSize()
        {
            const int batchSize = 10;

            await ArrangeAct(batchSize).ConfigureAwait(false);

            AssertPullingConsumer(batchSize, batchSize, 0);
        }

        [Fact]
        public async Task LessThanBatchSize()
        {
            const int batchSize    = 10;
            const int messageCount = 2;

            await ArrangeAct(batchSize, messageCount: messageCount).ConfigureAwait(false);

            AssertPullingConsumer(messageCount + 1, messageCount, 0);
        }

        [Theory]
        [InlineData(20,  10)]
        [InlineData(10,  100)]
        [InlineData(20,  20)]
        [InlineData(100, 10)]
        public async Task ProcessingFailures(int success, int fail)
        {
            var successCounter = 0;

            Task Process(string _, CancellationToken __)
            {
                if (++successCounter > success)
                {
                    throw new("Test Error");
                }

                return Task.CompletedTask;
            }

            var batchSize = success + fail;
            await ArrangeAct(batchSize, processor: Process).ConfigureAwait(false);

            AssertPullingConsumer(batchSize, success, fail);
        }

        [Fact]
        public async Task ReadError()
        {
            var readErrorCounter = 0;
            void ReadErrorHandler(Exception _, string __) => readErrorCounter++;

            var batchSize = 10;
            var consumer = Arrange<EasyNetQBatchConsumer<string>>(new(new()
            {
                BatchSize              = batchSize,
                MaxDegreeOfParallelism = 2
            }, int.MaxValue));
            ArrangeFaultySerializer();

            await Act(consumer, readErrorHandler: ReadErrorHandler).ConfigureAwait(false);

            Assert.Equal(batchSize, readErrorCounter);
            AssertPullingConsumer(batchSize, batchSize, 0);
        }

        private Task ArrangeAct(int                                    batchSize,
                                int                                    messageCount     = -1,
                                Func<string, CancellationToken, Task>? processor        = null,
                                Action<Exception, string>?             readErrorHandler = null)
        {
            return ArrangeAct(new(new()
            {
                BatchSize              = batchSize,
                MaxDegreeOfParallelism = 2
            }, messageCount < 0 ? int.MaxValue : messageCount), processor, readErrorHandler);
        }

        private Task ArrangeAct(TestOptions                            options,
                                Func<string, CancellationToken, Task>? processor        = null,
                                Action<Exception, string>?             readErrorHandler = null)
        {
            // Arrange
            var consumer = Arrange<EasyNetQBatchConsumer<string>>(options);

            // Act
            return Act(consumer, processor, readErrorHandler);
        }

        private static async Task Act(EasyNetQBatchConsumer<string>?         consumer,
                                      Func<string, CancellationToken, Task>? processor        = null,
                                      Action<Exception, string>?             readErrorHandler = null)
        {
            processor        ??= (s,         token) => Task.CompletedTask;
            readErrorHandler ??= (exception, s) => { };

            await consumer.StartAsync(processor, readErrorHandler, default).ConfigureAwait(false);
            await consumer.WaitForBatchAsync(default).ConfigureAwait(false);
            await consumer.StopAsync(default).ConfigureAwait(false);
        }

        private void AssertPullingConsumer(int pullCount, int ackCount, int rejectCount)
        {
            Assert.InRange(pullCount, ackCount + rejectCount, pullCount);

            PullingConsumerMock.Verify(
                consumer => consumer.AckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Exactly(ackCount));

            PullingConsumerMock.Verify(
                consumer => consumer.RejectAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(),
                                                 It.IsAny<CancellationToken>()),
                Times.Exactly(rejectCount));

            PullingConsumerMock.Verify(pullingConsumer => pullingConsumer.PullAsync(It.IsAny<CancellationToken>()),
                                       Times.Exactly(pullCount));
        }
    }
}
