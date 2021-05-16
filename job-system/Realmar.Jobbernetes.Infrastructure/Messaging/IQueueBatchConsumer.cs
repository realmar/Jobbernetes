using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IQueueBatchConsumer<TData> : IQueueConsumer<TData>
    {
        Task StartAsync(Func<TData, CancellationToken, Task> processor,
                        Action<Exception, string>?           readHandler,
                        CancellationToken                    cancellationToken);

        Task WaitForBatchAsync(CancellationToken cancellationToken);
    }
}
