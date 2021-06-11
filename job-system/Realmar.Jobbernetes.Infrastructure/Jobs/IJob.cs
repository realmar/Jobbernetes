using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Jobs
{
    /// <summary>
    ///     Jobbernetes Job
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public interface IJob<in TData>
    {
        /// <summary>
        ///     Processes data given by the framework.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task ProcessAsync(TData data, CancellationToken cancellationToken);
    }
}
