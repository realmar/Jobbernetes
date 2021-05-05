using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    internal class KafkaQueueProducer<TData> : IQueueProducer<TData>
    {
        private readonly ILogger<KafkaQueueProducer<TData>> _logger;
        private readonly IProducer<Null, TData>             _producer;
        private readonly string                             _topic;

        public KafkaQueueProducer(IOptions<KafkaOptions>             options,
                                  IProducer<Null, TData>             producer,
                                  ILogger<KafkaQueueProducer<TData>> logger)
        {
            _producer = producer;
            _logger   = logger;
            _topic    = options.Value.Topic;
        }

        /// <exception cref="T:Confluent.Kafka.ProduceException`2">When producing the message fails.</exception>
        /// <inheritdoc />
        public async Task Produce(TData data)
        {
            try
            {
                var result = await _producer.ProduceAsync(_topic, new() { Value = data });
                _logger.LogInformation($"Delivered data to Kafka TopicPartitionOffset = {result.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, TData> e)
            {
                _logger.LogError($"Failed to deliver data to Kafka Error.Reason = {e.Error.Reason}");
                throw;
            }
        }
    }
}
