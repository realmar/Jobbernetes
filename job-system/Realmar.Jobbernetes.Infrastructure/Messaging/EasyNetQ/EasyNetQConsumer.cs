using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options.RabbitMQ;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal class EasyNetQConsumer<TData> : IQueueConsumer<TData>
    {
        private readonly IBus                              _bus;
        private readonly IOptions<RabbitMQConsumerOptions> _rabbitMqOptions;
        private          IDisposable?                      _subscription;

        public EasyNetQConsumer(IBus                              bus,
                                IOptions<RabbitMQConsumerOptions> rabbitMqOptions)
        {
            _bus             = bus;
            _rabbitMqOptions = rabbitMqOptions;
        }

        public async Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken)
        {
            var queue = await _bus.DeclareAndBindQueueAsync(_rabbitMqOptions, cancellationToken).ConfigureAwait(false);

            _subscription = _bus.Advanced.Consume<TData>(
                queue,
                (message, _) => processor.Invoke(message.Body, cancellationToken));
        }

        public Task StopAsync(CancellationToken _)
        {
            _subscription?.Dispose();
            return Task.CompletedTask;
        }
    }
}
