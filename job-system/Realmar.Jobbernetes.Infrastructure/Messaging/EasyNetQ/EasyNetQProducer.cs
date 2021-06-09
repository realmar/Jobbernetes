using System.Threading;
using System.Threading.Tasks;
using DotNext.Threading;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Infrastructure.Options.RabbitMQ;
using ISerializer = Realmar.Jobbernetes.Infrastructure.Messaging.Serialization.ISerializer;

namespace Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ
{
    internal class EasyNetQProducer<TData> : IQueueProducer<TData>
    {
        private readonly IBus                              _bus;
        private readonly AsyncLazy<IExchange>              _exchange;
        private readonly IOptions<RabbitMQProducerOptions> _options;
        private readonly ISerializer                       _serializer;
        private readonly ITypeNameSerializer               _typeNameSerializer;

        public EasyNetQProducer(IBus                              bus,
                                ISerializer                       serializer,
                                ITypeNameSerializer               typeNameSerializer,
                                IOptions<RabbitMQProducerOptions> options)
        {
            _bus                = bus;
            _serializer         = serializer;
            _typeNameSerializer = typeNameSerializer;
            _options            = options;

            _exchange = new(async () => (await bus
                                              .DeclareAndBindQueueAsync(options, default)
                                              .ConfigureAwait(false)).Item1);
        }

        public async Task ProduceAsync(TData data, CancellationToken cancellationToken)
        {
            var bytes = _serializer.Serialize(data);

            var exchange = await _exchange.ConfigureAwait(false);

            await _bus.Advanced
                      .PublishAsync(exchange,
                                    _options.Value.RoutingKey,
                                    mandatory: false,
                                    new()
                                    {
                                        DeliveryMode = 2,
                                        Type         = _typeNameSerializer.Serialize(typeof(TData)),
                                        ContentType  = _serializer.ContentType
                                    },
                                    bytes.ToArray(),
                                    cancellationToken)
                      .ConfigureAwait(false);
        }
    }
}
