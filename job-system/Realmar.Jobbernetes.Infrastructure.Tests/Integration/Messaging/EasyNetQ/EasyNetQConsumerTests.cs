using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Integration.Messaging.EasyNetQ
{
    public class EasyNetQConsumerTests : EasyNetQBaseTest
    {
        [Fact]
        public async Task ConsumeAsync()
        {
            var                     consumer = Arrange<EasyNetQConsumer<string>>();
            IMessageHandler<string> handler  = null;

            AdvancedBusMock.Setup(bus => bus.Consume(It.IsAny<IQueue>(),
                                                     It.IsAny<IMessageHandler<string>>(),
                                                     It.IsAny<Action<IConsumerConfiguration>>()))
                           .Callback<IQueue, IMessageHandler<string>, Action<IConsumerConfiguration>>(
                                (_, onMessage, _) => handler = onMessage);

            var called = false;
            await consumer.StartAsync((_, _) =>
            {
                called = true;
                return Task.CompletedTask;
            }, default).ConfigureAwait(false);

            Assert.NotNull(handler);

            await handler.Invoke(new Message<string>("Hello World"), default, default)
                         .ConfigureAwait(false);

            Assert.True(called);
        }
    }
}
