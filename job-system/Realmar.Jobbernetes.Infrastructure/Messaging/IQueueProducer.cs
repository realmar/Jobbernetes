using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Messaging
{
    /// <summary>
    ///     Produces messages.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public interface IQueueProducer<in TData>
    {
        /// <summary>
        ///     Write message to the queue.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task ProduceAsync(TData data, CancellationToken cancellationToken);
    }
}
