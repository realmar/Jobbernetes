using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Facade
{
    /// <summary>
    ///     Jobbernetes Facade
    /// </summary>
    public interface IJobbernetes
    {
        /// <summary>
        ///     Runs a hosted Jobbernetes job.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task Run(CancellationToken cancellationToken);
    }
}
