using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal class EasyNetQProducer<TData> : EasyNetQBase, IQueueProducer<TData>
    {
        private readonly IBus                              _bus;
        private readonly IOptions<RabbitMQProducerOptions> _options;

        public EasyNetQProducer(IOptions<RabbitMQProducerOptions> options, IBus bus) : base(options, bus)
        {
            _bus     = bus;
            _options = options;
        }

        public async Task ProduceAsync(TData data, CancellationToken cancellationToken)
        {
            await PrepareCommunication(cancellationToken).ConfigureAwait(false);
            await _bus.Advanced
                      .PublishAsync(Exchange,
                                    _options.Value.RoutingKey,
                                    mandatory: false,
                                    new Message<TData>(data),
                                    cancellationToken)
                      .ConfigureAwait(false);
        }
    }
}
