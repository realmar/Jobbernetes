using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    internal class KafkaQueueConsumer<TData> : IQueueConsumer<TData>
    {
        private readonly IConsumer<Ignore, TData>           _consumer;
        private readonly ILogger<KafkaQueueConsumer<TData>> _logger;
        private readonly TimeSpan                           _pollTimeout;
        private readonly string                             _topic;

        public KafkaQueueConsumer(IOptions<KafkaOptions>             options,
                                  IConsumer<Ignore, TData>           consumer,
                                  ILogger<KafkaQueueConsumer<TData>> logger)
        {
            _consumer    = consumer;
            _logger      = logger;
            _pollTimeout = TimeSpan.FromSeconds(options.Value.PollTimeout);
            _topic       = options.Value.Topic;
        }

        public async IAsyncEnumerable<TData> Consume([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            while (true)
            {
                TData? data    = default;
                var    success = false;
                try
                {
                    data    = await ConsumeMessage(cancellationToken).ConfigureAwait(false);
                    success = true;
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Failed to consume Kafka message Error.Reason = {e.Error.Reason} Exception = {e}");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation($"Kafka Topic = {_topic} is empty or cancellation was requested");
                    _consumer.Unsubscribe();

                    yield break;
                }

                if (success)
                {
                    if (data != null)
                    {
                        yield return data;
                    }
                    else
                    {
                        _logger.LogWarning($"Kafka read a null value on Topic = {_topic}, message discarded");
                    }
                }
            }
        }

        private async Task<TData?> ConsumeMessage(CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_pollTimeout);

            var result = await Task.Run(() => _consumer.Consume(cts.Token), cts.Token)
                                   .ConfigureAwait(false);

            if (result != null)
            {
                _logger.LogInformation(
                    $"Read message from Kafka Topic = {_topic}, TopicPartitionOffset = {result.TopicPartitionOffset}");
            }

            return result == null ? default : result.Message.Value;
        }
    }
}
