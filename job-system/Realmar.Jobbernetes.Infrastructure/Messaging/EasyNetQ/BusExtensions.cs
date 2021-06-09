using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Infrastructure.Options.RabbitMQ;

namespace Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ
{
    internal static class BusExtensions
    {
        internal static async Task<(IExchange, IQueue)> DeclareAndBindQueueAsync(this IBus bus,
                                                                                 IOptions<RabbitMQPubSubOptions> options,
                                                                                 CancellationToken cancellationToken)
        {
            var exchange = await bus.DeclareExchangeAsync(options, cancellationToken).ConfigureAwait(false);
            var queue    = await bus.DeclareQueue(options, cancellationToken).ConfigureAwait(false);
            await bus.BindQueue(exchange, queue, options, cancellationToken).ConfigureAwait(false);

            return (exchange, queue);
        }

        private static Task<IExchange> DeclareExchangeAsync(this IBus                       bus,
                                                            IOptions<RabbitMQPubSubOptions> options,
                                                            CancellationToken               cancellationToken) =>
            bus.Advanced.ExchangeDeclareAsync(options.Value.Exchange, configuration =>
            {
                configuration.AsAutoDelete(false);
                configuration.AsDurable(true);
                configuration.WithType("direct");
            }, cancellationToken);

        private static Task<IQueue> DeclareQueue(this IBus                       bus,
                                                 IOptions<RabbitMQPubSubOptions> options,
                                                 CancellationToken               cancellationToken) =>
            bus.Advanced.QueueDeclareAsync(options.Value.Queue, configuration =>
            {
                configuration.AsAutoDelete(false);
                configuration.AsDurable(true);
            }, cancellationToken);

        private static Task<IBinding> BindQueue(this IBus                       bus,
                                                IExchange                       exchange,
                                                IQueue                          queue,
                                                IOptions<RabbitMQPubSubOptions> options,
                                                CancellationToken               cancellationToken) =>
            bus.Advanced.BindAsync(exchange, queue, options.Value.BindingKey, cancellationToken);
    }
}
