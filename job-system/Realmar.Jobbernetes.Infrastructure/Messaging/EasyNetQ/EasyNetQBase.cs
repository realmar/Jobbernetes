using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options.RabbitMQ;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal abstract class EasyNetQBase
    {
        private readonly IBus                            _bus;
        private readonly IOptions<RabbitMQPubSubOptions> _options;
#pragma warning disable IDE0052 // Remove unread private members
        private IBinding? _binding;
#pragma warning restore IDE0052 // Remove unread private members

        protected EasyNetQBase(IOptions<RabbitMQPubSubOptions> options, IBus bus)
        {
            _options = options;
            _bus     = bus;
        }

        protected IExchange? Exchange { get; private set; }

        protected IQueue? Queue { get; private set; }

        protected Task PrepareCommunication(CancellationToken cancellationToken)
        {
            if (Exchange == null || Queue == null)
            {
                return DeclareAndBind(cancellationToken);
            }

            return Task.CompletedTask;
        }

        private async Task DeclareAndBind(CancellationToken cancellationToken)
        {
            Queue = await _bus.Advanced.QueueDeclareAsync(_options.Value.Queue, configuration =>
            {
                configuration.AsAutoDelete(false);
                configuration.AsDurable(true);
            }, cancellationToken).ConfigureAwait(false);

            Exchange = await _bus.Advanced.ExchangeDeclareAsync(_options.Value.Exchange, configuration =>
            {
                configuration.AsAutoDelete(false);
                configuration.AsDurable(true);
                configuration.WithType("direct");
            }, cancellationToken).ConfigureAwait(false);

            _binding = await _bus.Advanced.BindAsync(Exchange, Queue, _options.Value.RoutingKey, cancellationToken)
                                 .ConfigureAwait(false);
        }

        protected string FormatLog(string message) => $"{message} "                            +
                                                      $"Exchange = {_options.Value.Exchange} " +
                                                      $"Queue = {_options.Value.Queue} "       +
                                                      $"RoutingKey = {_options.Value.RoutingKey}";
    }
}
