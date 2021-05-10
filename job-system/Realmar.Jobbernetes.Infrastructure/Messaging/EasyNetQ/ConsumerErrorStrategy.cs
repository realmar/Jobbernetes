using System;
using EasyNetQ.Consumer;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    public class ConsumerErrorStrategy : IConsumerErrorStrategy
    {
        public void Dispose() { }

        public AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            return AckStrategies.NackWithRequeue;
        }

        public AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            return AckStrategies.NackWithRequeue;
        }
    }
}
