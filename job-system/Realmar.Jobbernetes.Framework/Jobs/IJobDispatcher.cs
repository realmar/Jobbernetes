using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Jobs
{
    public interface IJobDispatcher<in TData>
    {
        Task Dispatch(TData data, CancellationToken cancellationToken);
    }
}
