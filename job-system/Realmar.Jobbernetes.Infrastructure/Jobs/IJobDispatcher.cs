using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Jobs
{
    internal interface IJobDispatcher<in TData>
    {
        Task Dispatch(TData data, CancellationToken cancellationToken = default);
    }
}
