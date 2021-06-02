using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Jobs
{
    public interface IJob<in TData>
    {
        Task ProcessAsync(TData data, CancellationToken cancellationToken);
    }
}
