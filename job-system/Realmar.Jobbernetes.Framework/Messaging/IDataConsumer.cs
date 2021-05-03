using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IDataConsumer<in TData>
    {
        Task Consume(TData data, CancellationToken cancellationToken);
    }
}
