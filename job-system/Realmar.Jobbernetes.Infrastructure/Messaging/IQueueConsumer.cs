using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Messaging
{
    public interface IQueueConsumer<TData>
    {
        Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken);
        Task StopAsync(CancellationToken                     cancellationToken);
    }
}
