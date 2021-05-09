using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal class ChannelProvider
    {
        public delegate ChannelProvider Factory(IModel channel);

        private readonly IConnection              _connection;
        private readonly ILogger<ChannelProvider> _logger;
        private          IModel?                  _channel;

        public ChannelProvider(IConnection connection, ILogger<ChannelProvider> logger, IModel? channel = null)
        {
            _connection = connection;
            _logger     = logger;
            _channel    = channel;
        }

        public IModel GetChannel()
        {
            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogInformation("Creating new RabbitMQ channel");

                _channel?.Dispose();
                _channel = _connection.CreateModel();
            }

            return _channel;
        }
    }
}
