using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;
using Realmar.Jobbernetes.Framework.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal class RabbitMQConsumer<TData> : RabbitMQBaseConsumer<TData>, IQueueConsumer<TData>
    {
        private readonly Owned<ChannelProvider>            _channelProvider;
        private readonly ILogger<RabbitMQConsumer<TData>>  _logger;
        private readonly IOptions<RabbitMQConsumerOptions> _options;

        public RabbitMQConsumer(
            ChannelProvider.Factory channelFactory,
            RabbitMQMessageCommitter.Factory committerFactory,
            Owned<ChannelProvider> channelProvider,
            IDeserializer<TData> deserializer,
            IOptions<RabbitMQConsumerOptions> options,
            ILogger<RabbitMQConsumer<TData>> logger,
            IMetricsNameFactory nameFactory) : base(options, channelFactory, committerFactory, deserializer, logger,
                                                    nameFactory)
        {
            _channelProvider = channelProvider;
            _options         = options;
            _logger          = logger;
        }

        public async IAsyncEnumerable<Message<TData>> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await PrepareChannel(_channelProvider.Value.GetChannel(), cancellationToken).ConfigureAwait(false);

            while (cancellationToken.IsCancellationRequested == false)
            {
                var             channel = _channelProvider.Value.GetChannel();
                BasicGetResult? result  = null;

                try
                {
                    lock (channel)
                    {
                        result = channel.BasicGet(_options.Value.Queue, autoAck: false);
                    }
                }
                catch (Exception e)
                {
                    Counter.WithFail().Inc();
                    _logger.LogError(e, "Failed to get message from RabbitMQ");
                }

                if (result != null)
                {
                    var data = await ProcessMessage(channel, result, cancellationToken).ConfigureAwait(false);
                    if (data != null)
                    {
                        yield return data.Value;
                    }
                }
                else
                {
                    _logger.LogInformation("All messages processed");
                    yield break;
                }
            }
        }
    }
}
