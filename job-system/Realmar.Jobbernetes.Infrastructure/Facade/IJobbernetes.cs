using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Facade
{
    public interface IJobbernetes
    {
        Task Run(CancellationToken cancellationToken);
    }
}
