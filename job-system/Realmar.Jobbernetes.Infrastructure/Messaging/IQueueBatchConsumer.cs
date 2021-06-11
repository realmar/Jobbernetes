using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Messaging
{
    /// <summary>
    ///     Consumes messages from the queue using a specified max batch size.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="Realmar.Jobbernetes.Infrastructure.Messaging.IQueueConsumer{TData}" />
    public interface IQueueBatchConsumer<TData> : IQueueConsumer<TData>
    {
        /// <summary>
        ///     Starts processing.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <param name="readHandler">The read handler.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task StartAsync(Func<TData, CancellationToken, Task> processor,
                        Action<Exception, string>?           readHandler,
                        CancellationToken                    cancellationToken);

        /// <summary>
        ///     Waits for batch to be fully processed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task WaitForBatchAsync(CancellationToken cancellationToken);
    }
}
