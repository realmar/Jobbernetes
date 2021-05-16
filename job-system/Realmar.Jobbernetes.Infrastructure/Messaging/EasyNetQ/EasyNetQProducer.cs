using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options.RabbitMQ;
using ISerializer = Realmar.Jobbernetes.Framework.Messaging.Serialization.ISerializer;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal class EasyNetQProducer<TData> : EasyNetQBase, IQueueProducer<TData>
    {
        private readonly IBus                              _bus;
        private readonly IOptions<RabbitMQProducerOptions> _options;
        private readonly ISerializer                       _serializer;
        private readonly ITypeNameSerializer               _typeNameSerializer;

        public EasyNetQProducer(IBus                              bus,
                                ISerializer                       serializer,
                                ITypeNameSerializer               typeNameSerializer,
                                IOptions<RabbitMQProducerOptions> options) : base(options, bus)
        {
            _bus                = bus;
            _serializer         = serializer;
            _typeNameSerializer = typeNameSerializer;
            _options            = options;
        }

        public async Task ProduceAsync(TData data, CancellationToken cancellationToken)
        {
            await PrepareCommunication(cancellationToken).ConfigureAwait(false);

            var bytes = _serializer.Serialize(data);

            await _bus.Advanced
                      .PublishAsync(Exchange,
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
