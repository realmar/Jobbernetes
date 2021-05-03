using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Framework
{
    public class Jobbernetes<TData> : IJobbernetes
    {
        private readonly IDataConsumer<TData> _consumer;
        private readonly List<Task>           _consumers = new();
        private readonly IDataProducer<TData> _producer;

        public Jobbernetes(IDataConsumer<TData> consumer, IDataProducer<TData> producer)
        {
            _consumer = consumer;
            _producer = producer;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await foreach (var data in _producer.Produce(cancellationToken).ConfigureAwait(false))
            {
                var task = _consumer.Consume(data, cancellationToken);
                _consumers.Add(task);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            // ReSharper is wrong, it is needed because of the await foreach
            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await Task.WhenAll(_consumers).ConfigureAwait(false);
        }
    }
}
