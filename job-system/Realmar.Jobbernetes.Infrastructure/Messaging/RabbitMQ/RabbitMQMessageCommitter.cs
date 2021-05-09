using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal class RabbitMQMessageCommitter : IMessageCommitter
    {
        private readonly ChannelProvider _channelProvider;
        private readonly BasicGetResult  _result;

        public RabbitMQMessageCommitter(BasicGetResult result, ChannelProvider channelProvider)
        {
            _result          = result;
            _channelProvider = channelProvider;
        }

        public Task CommitAsync(CancellationToken cancellationToken) =>
            Task.Run(() => Execute(channel => channel.BasicAck(_result.DeliveryTag, multiple: false)),
                     cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken) =>
            Task.Run(() => Execute(channel => channel.BasicNack(_result.DeliveryTag, multiple: false, requeue: true)),
                     cancellationToken);

        private void Execute(Action<IModel> action)
        {
            var channel = _channelProvider.GetChannel();
            lock (channel)
            {
                action.Invoke(channel);
            }
        }

        internal delegate RabbitMQMessageCommitter Factory(BasicGetResult result, ChannelProvider channelProvider);
    }
}
