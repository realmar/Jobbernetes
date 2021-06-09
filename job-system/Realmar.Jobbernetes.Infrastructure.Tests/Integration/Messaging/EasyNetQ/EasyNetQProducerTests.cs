using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Integration.Messaging.EasyNetQ
{
    public class EasyNetQProducerTests : EasyNetQBaseTest
    {
        [Fact]
        public async Task Produce()
        {
            var producer = Arrange<EasyNetQProducer<string>>();

            await producer.ProduceAsync("Hello World", default).ConfigureAwait(false);

            AdvancedBusMock.Verify(bus => bus
                                      .PublishAsync(
                                           It.IsAny<IExchange>(),
                                           It.IsAny<string>(),
                                           It.IsAny<bool>(),
                                           It.IsAny<MessageProperties>(),
                                           It.IsAny<byte[]>(),
                                           It.IsAny<CancellationToken>()),
                                   Times.Once);
        }
    }
}
