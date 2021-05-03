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
    public class KafkaDataProducer<TData> : IDataProducer<TData>
    {
        private readonly IConsumer<Ignore, TData>          _consumer;
        private readonly ILogger<KafkaDataProducer<TData>> _logger;
        private readonly string                            _topic;

        public KafkaDataProducer(IOptions<KafkaOptions>            options,
                                 IConsumer<Ignore, TData>          consumer,
                                 ILogger<KafkaDataProducer<TData>> logger)
        {
            _consumer = consumer;
            _logger   = logger;
            _topic    = options.Value.Topic;
        }

        public async IAsyncEnumerable<TData> Produce([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            while (true)
            {
                TData? data = default;

                try
                {
                    var result = await Task.Run(() => _consumer.Consume(cancellationToken), cancellationToken)
                                           .ConfigureAwait(false);
                    Console.WriteLine($"Consumed message at {result.TopicPartitionOffset}");

                    data = result.Message.Value;

                    _logger.LogInformation(
                        $"Read message from Kafka Topic = {_topic}, TopicPartitionOffset = {result.TopicPartitionOffset}");
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Failed to consume Kafka message Error.Reason = {e.Error.Reason}");
                }
                catch (OperationCanceledException e)
                {
                    _logger.LogError($"Operation cancelled while consuming Kafka Topic = {_topic} Error = {e.Message}");
                }

                if (data != null)
                {
                    yield return data;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
}
