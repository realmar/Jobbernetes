using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using RabbitMQ.Client;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;
using Realmar.Jobbernetes.Framework.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal abstract class RabbitMQBaseConsumer<TData> : RabbitMQBase
    {
        private readonly ChannelProvider.Factory          _channelFactory;
        private readonly RabbitMQMessageCommitter.Factory _committerFactory;
        private readonly IDeserializer<TData>             _deserializer;
        private readonly ILogger                          _logger;

        protected RabbitMQBaseConsumer(IOptions<RabbitMQPubSubOptions>  options,
                                       ChannelProvider.Factory          channelFactory,
                                       RabbitMQMessageCommitter.Factory committerFactory,
                                       IDeserializer<TData>             deserializer,
                                       ILogger                          logger,
                                       IMetricsNameFactory              nameFactory) : base(options)
        {
            _committerFactory = committerFactory;
            _deserializer     = deserializer;
            _logger           = logger;
            _channelFactory   = channelFactory;

            var name = nameFactory.Create("rabbitmq_consumer");
            Counter = Metrics.CreateCounter(name, "Number of consumed messages", Labels.Keys.Status);
        }

        protected Counter Counter { get; }

        protected async Task<Message<TData>?> ProcessMessage(IModel            channel,
                                                             BasicGetResult    result,
                                                             CancellationToken cancellationToken)
        {
            Message<TData>? message = null;
            var             data    = result.Body;
            TData?          obj     = default;

            try
            {
                obj = await _deserializer.DeserializeAsync(data, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                ProcessDeserializationFailure(channel, result, e);
            }

            if (obj == null)
            {
                ProcessDeserializationFailure(channel, result);
            }
            else
            {
                Counter.WithSuccess().Inc();
                message = new(obj, _committerFactory.Invoke(result, _channelFactory.Invoke(channel)));
            }

            _logger.LogInformation("Read message from queue");

            return message;
        }

        private void ProcessDeserializationFailure(IModel         channel,
                                                   BasicGetResult result,
                                                   Exception?     deserializationException = null)
        {
            var message = FormatLog($"Cannot deserialize data Length = {result.Body.Length} ");
            if (deserializationException != null)
            {
                _logger.LogError(deserializationException, message);
            }
            else
            {
                _logger.LogError(message);
            }

            try
            {
                lock (channel)
                {
                    channel.BasicAck(result.DeliveryTag, multiple: false);
                }

                Counter.WithLabels(Labels.Values.FailDeserialization).Inc();
            }
            catch (Exception ackException)
            {
                Counter.WithFailCommit().Inc();
                _logger.LogError(ackException, FormatLog("Failed to commit non-serializable message"));
            }
        }
    }
}
