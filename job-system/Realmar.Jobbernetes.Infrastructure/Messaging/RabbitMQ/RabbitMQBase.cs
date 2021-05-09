using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    public abstract class RabbitMQBase
    {
        private readonly IOptions<RabbitMQPubSubOptions> _options;
        private          bool                            _preparedChannel;

        protected RabbitMQBase(IOptions<RabbitMQPubSubOptions> options) => _options = options;

        protected Task PrepareChannel(IModel channel, CancellationToken cancellationToken)
        {
            if (_preparedChannel)
            {
                return Task.CompletedTask;
            }

            return DeclareAndBind(channel, cancellationToken);
        }

        private Task DeclareAndBind(IModel channel, CancellationToken cancellationToken) => Task.Run(() =>
            {
                channel.QueueDeclare(queue: _options.Value.Queue,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.ExchangeDeclare(
                    exchange: _options.Value.Exchange,
                    type: "direct",
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                channel.QueueBind(
                    queue: _options.Value.Queue,
                    exchange: _options.Value.Exchange,
                    routingKey: _options.Value.RoutingKey,
                    arguments: null);

                _preparedChannel = true;
            },
            cancellationToken);

        protected string FormatLog(string message) => $"{message} "                            +
                                                      $"Exchange = {_options.Value.Exchange} " +
                                                      $"Queue = {_options.Value.Queue} "       +
                                                      $"RoutingKey = {_options.Value.RoutingKey}";
    }
}
