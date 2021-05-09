using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IQueueProducer<in TData>
    {
        Task ProduceAsync(TData data, CancellationToken cancellationToken);
    }
}
