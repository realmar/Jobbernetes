using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using RabbitMQ.Client;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;
using Realmar.Jobbernetes.Framework.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal class RabbitMQProducer<TData> : RabbitMQBase, IQueueProducer<TData>, IDisposable
    {
        private readonly Owned<ChannelProvider>            _channelProvider;
        private readonly Counter                           _counter;
        private readonly ILogger<RabbitMQProducer<TData>>  _logger;
        private readonly IOptions<RabbitMQProducerOptions> _options;
        private readonly ISerializer<TData>                _serializer;


        public RabbitMQProducer(Owned<ChannelProvider>            channelProvider,
                                IOptions<RabbitMQProducerOptions> options,
                                ISerializer<TData>                serializer,
                                ILogger<RabbitMQProducer<TData>>  logger,
                                IMetricsNameFactory               nameFactory) : base(options)
        {
            _channelProvider = channelProvider;
            _options         = options;
            _serializer      = serializer;
            _logger          = logger;

            var name = nameFactory.Create("rabbitmq_producer");
            _counter = Metrics.CreateCounter(name, "Number of produced messages", Labels.Keys.Status);
        }

        public void Dispose()
        {
            _channelProvider.Dispose();
        }

        public async Task ProduceAsync(TData data, CancellationToken cancellationToken)
        {
            var channel = _channelProvider.Value.GetChannel();

            await PrepareChannel(channel, cancellationToken).ConfigureAwait(false);

            var bytes = await SerializeMessage(data, cancellationToken).ConfigureAwait(false);

            try
            {
                ProduceMessage(channel, bytes);
                _logger.LogInformation("Produced message");
            }
            catch (Exception e)
            {
                _counter.WithFail().Inc();
                _logger.LogError(e, FormatLog("Failed to produce message"));

                throw;
            }
        }

        private async Task<ReadOnlyMemory<byte>> SerializeMessage(TData data, CancellationToken cancellationToken)
        {
            try
            {
                return await _serializer.SerializeAsync(data, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _counter.WithLabels(Labels.Values.FailSerialization).Inc();
                _logger.LogError(e, FormatLog("Failed to serialize message"));

                throw;
            }
        }

        private void ProduceMessage(IModel channel, ReadOnlyMemory<byte> bytes)
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: _options.Value.Exchange,
                routingKey: _options.Value.RoutingKey,
                basicProperties: properties,
                body: bytes);

            _counter.WithSuccess().Inc();
        }
    }
}
