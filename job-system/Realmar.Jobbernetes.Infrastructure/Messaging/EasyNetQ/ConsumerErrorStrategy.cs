using System;
using EasyNetQ.Consumer;
using Microsoft.Extensions.Logging;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal class ConsumerErrorStrategy : IConsumerErrorStrategy
    {
        private readonly ILogger<ConsumerErrorStrategy> _logger;

        public ConsumerErrorStrategy(ILogger<ConsumerErrorStrategy> logger) => _logger = logger;

        public void Dispose() { }

        public AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            _logger.LogInformation(exception, $"Processing consumer error: {context.ReceivedInfo}");
            return AckStrategies.NackWithRequeue;
        }

        public AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            _logger.LogInformation($"Processing consumer cancelled: {context.ReceivedInfo}");
            return AckStrategies.NackWithRequeue;
        }
    }
}
