using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class KafkaDataSender<TData> : IDataSender<TData>
    {
        private readonly ILogger<KafkaDataSender<TData>> _logger;
        private readonly IProducer<Null, TData>          _producer;
        private readonly string                          _topic;

        public KafkaDataSender(IOptions<KafkaOptions>          options,
                               IProducer<Null, TData>          producer,
                               ILogger<KafkaDataSender<TData>> logger)
        {
            _producer = producer;
            _logger   = logger;
            _topic    = options.Value.Topic;
        }

        public async Task Send(TData data)
        {
            try
            {
                var result = await _producer.ProduceAsync(_topic, new() { Value = data });
                _logger.LogInformation($"Delivered data to Kafka TopicPartitionOffset = {result.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, TData> e)
            {
                _logger.LogError($"Failed to deliver data to Kafka Error.Reason = {e.Error.Reason}");
            }
        }
    }
}
