using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Messaging
{
    /// <summary>
    ///     Consumes messages from the queue until stop is called.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public interface IQueueConsumer<TData>
    {
        /// <summary>
        ///     Starts consuming messages.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken);

        /// <summary>
        ///     Stops consuming messages.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
