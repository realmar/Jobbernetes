using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;
using Realmar.Jobbernetes.Framework.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal class RabbitMQEndlessConsumer<TData> : RabbitMQBaseConsumer<TData>, IQueueConsumer<TData>
    {
        private readonly Owned<ChannelProvider>                  _channelProvider;
        private readonly ILogger<RabbitMQEndlessConsumer<TData>> _logger;
        private readonly IOptions<RabbitMQConsumerOptions>       _options;

        public RabbitMQEndlessConsumer(
            IOptions<RabbitMQConsumerOptions>       options,
            ChannelProvider.Factory                 channelFactory,
            RabbitMQMessageCommitter.Factory        committerFactory,
            IDeserializer<TData>                    deserializer,
            ILogger<RabbitMQEndlessConsumer<TData>> logger,
            IMetricsNameFactory                     nameFactory,
            Owned<ChannelProvider>                  channelProvider) :
            base(options, channelFactory, committerFactory, deserializer, logger, nameFactory)
        {
            _logger          = logger;
            _channelProvider = channelProvider;
            _options         = options;
        }


        public async IAsyncEnumerable<Message<TData>> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                var buffer = new BufferBlock<BasicDeliverEventArgs>();

                var channel = _channelProvider.Value.GetChannel();
                await PrepareChannel(channel, cancellationToken).ConfigureAwait(false);

                var client = new AsyncEventingBasicConsumer(channel);
                client.Received += (_, @event) =>
                {
                    _logger.LogInformation(FormatLog("Received message"));
                    buffer.Post(@event);

                    return Task.CompletedTask;
                };
                client.Shutdown += (_, @event) =>
                {
                    _logger.LogInformation(FormatLog(
                                               "Channel closed "                  +
                                               $"ReplyCode = {@event.ReplyCode} " +
                                               $"ReplyText = {@event.ReplyText}"));

                    return Task.CompletedTask;
                };

                channel.BasicConsume(
                    queue: _options.Value.Queue,
                    autoAck: false,
                    consumerTag: string.Empty,
                    noLocal: default,
                    exclusive: default,
                    arguments: null!,
                    consumer: client);

                while (await buffer.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
                {
                    var @event = await buffer.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                    var result = new BasicGetResult(
                        deliveryTag: @event.DeliveryTag,
                        redelivered: @event.Redelivered,
                        exchange: @event.Exchange,
                        routingKey: @event.RoutingKey,
                        messageCount: 1,
                        basicProperties: @event.BasicProperties,
                        body: @event.Body);

                    var data = await ProcessMessage(channel, result, cancellationToken).ConfigureAwait(false);
                    if (data != null)
                    {
                        yield return data.Value;
                    }
                }
            }
        }
    }
}
