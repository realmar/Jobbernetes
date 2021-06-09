using System;
using Autofac.Extras.Moq;
using EasyNetQ.Consumer;
using Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Messaging.EasyNetQ
{
    public class ConsumerErrorStrategyTests : IDisposable
    {
        private readonly AutoMock _mock = AutoMock.GetLoose();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _mock.Dispose();
        }

        [Fact]
        public void HandleConsumerError()
        {
            Assert.Equal(AckStrategies.NackWithRequeue,
                         _mock.Create<ConsumerErrorStrategy>().HandleConsumerError(default, default));
        }

        [Fact]
        public void HandleConsumerCancelled()
        {
            Assert.Equal(AckStrategies.NackWithRequeue,
                         _mock.Create<ConsumerErrorStrategy>().HandleConsumerCancelled(default));
        }
    }
}
