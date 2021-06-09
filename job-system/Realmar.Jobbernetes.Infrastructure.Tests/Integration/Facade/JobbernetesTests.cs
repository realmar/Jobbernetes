using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using DotNext.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Realmar.Jobbernetes.Infrastructure.Facade;
using Realmar.Jobbernetes.Infrastructure.Jobs;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Realmar.Jobbernetes.Infrastructure.Options.Metrics;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Integration.Facade
{
    public class JobbernetesTests
    {
        [Fact]
        public async Task Run()
        {
            // Arrange
            using var mock = AutoMock.GetLoose(builder => builder.RegisterModule<MetricsModule>());

            const string data                = "Hello World";
            var          tcs                 = new TaskCompletionSource();
            var          metricPusherOptions = new MetricPusherOptions();

            Func<string, CancellationToken, Task>? processor   = null;
            Action<Exception, string>?             readHandler = null;
            var                                    reset       = new AsyncAutoResetEvent(false);

            mock.Mock<IOptions<MetricsOptions>>()
                .Setup(options => options.Value)
                .Returns(new MetricsOptions { BasePrefix = "TestMetrics" });

            mock.Mock<IOptions<MetricPusherOptions>>()
                .Setup(options => options.Value)
                .Returns(metricPusherOptions);

            mock.Mock<IJobDispatcher<string>>()
                .Setup(dispatcher => dispatcher.Dispatch(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((_, _) => Task.CompletedTask);

            mock.Mock<IQueueBatchConsumer<string>>()
                .Setup(consumer => consumer.StartAsync(It.IsAny<Func<string, CancellationToken, Task>>(),
                                                       It.IsAny<Action<Exception, string>?>(),
                                                       It.IsAny<CancellationToken>()))
                .Returns<Func<string, CancellationToken, Task>, Action<Exception, string>?, CancellationToken>(
                     (p, rh, _) =>
                     {
                         (processor, readHandler) = (p, rh);
                         reset.Set();

                         return Task.CompletedTask;
                     });

            mock.Mock<IQueueBatchConsumer<string>>()
                .Setup(consumer => consumer.WaitForBatchAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ => tcs.Task);

            mock.Mock<IQueueBatchConsumer<string>>()
                .Setup(consumer => consumer.StopAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var jobbernetes =
                mock.Create<Jobbernetes<string>>(
                    new TypedParameter(typeof(IMetricFactory), new MetricFactory(new CollectorRegistry())));

            // Act
            var task = jobbernetes.Run(default);

            await reset.WaitAsync().ConfigureAwait(false);

            processor.Invoke(data, default);
            readHandler.Invoke(new("Test Error"), data);

            tcs.SetResult();

            await task.ConfigureAwait(false);

            // Assert
            mock.Mock<IJobDispatcher<string>>()
                .Verify(dispatcher => dispatcher.Dispatch(data, It.IsAny<CancellationToken>()), Times.Once);

            mock.Mock<ILogger<Jobbernetes<string>>>()
                .Verify(logger => logger.Log(LogLevel.Error,
                                             It.IsAny<EventId>(),
                                             It.IsAny<It.IsAnyType>(),
                                             It.IsAny<Exception>(),
                                             It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
        }
    }
}
