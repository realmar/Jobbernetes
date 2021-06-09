using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Messaging
{
    public interface IQueueProducer<in TData>
    {
        Task ProduceAsync(TData data, CancellationToken cancellationToken);
    }
}
