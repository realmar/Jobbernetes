using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    public class EasyNetQConsumer<TData> : EasyNetQBase, IQueueConsumer<TData>
    {
        private readonly IBus                        _bus;
        private readonly IOptions<ProcessingOptions> _processingOptions;
        private          IDisposable                 _subscription;

        public EasyNetQConsumer(IBus                              bus,
                                IOptions<RabbitMQConsumerOptions> rabbitMqOptions,
                                IOptions<ProcessingOptions>       processingOptions) : base(rabbitMqOptions, bus)
        {
            _bus               = bus;
            _processingOptions = processingOptions;
        }

        public async Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken)
        {
            await PrepareCommunication(cancellationToken).ConfigureAwait(false);
            var batchSize = (ushort) Math.Clamp(_processingOptions.Value.BatchSize + 8, ushort.MinValue, ushort.MaxValue);

            _subscription = _bus.Advanced.Consume<TData>(
                Queue,
                (message, _) => processor.Invoke(message.Body, cancellationToken),
                configuration => configuration.WithPrefetchCount(batchSize));
        }

        public Task StopAsync(CancellationToken _)
        {
            _subscription.Dispose();
            return Task.CompletedTask;
        }
    }
}
