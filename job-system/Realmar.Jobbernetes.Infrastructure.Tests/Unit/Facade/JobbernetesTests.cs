using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Facade;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Facade
{
    public class JobbernetesTests
    {
        [Fact]
        public async Task Run()
        {
            // Arrange
            using var mock         = AutoMock.GetLoose();
            var       consumerMock = mock.Mock<IQueueBatchConsumer<string>>();
            var       jobbernetes  = mock.Create<Jobbernetes<string>>();

            // Act
            await jobbernetes.Run(default).ConfigureAwait(false);

            // Assert
            consumerMock.Verify(consumer => consumer.StartAsync(
                                    It.IsAny<Func<string, CancellationToken, Task>>(),
                                    It.IsAny<Action<Exception, string>?>(),
                                    It.IsAny<CancellationToken>()),
                                Times.Once);

            consumerMock.Verify(consumer => consumer.WaitForBatchAsync(It.IsAny<CancellationToken>()),
                                Times.Once);

            consumerMock.Verify(consumer => consumer.StopAsync(It.IsAny<CancellationToken>()),
                                Times.Once);
        }
    }
}
