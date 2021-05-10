using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IQueueBatchConsumer<TData> : IQueueConsumer<TData>
    {
        Task WaitForBatchAsync(CancellationToken cancellationToken);
    }
}
