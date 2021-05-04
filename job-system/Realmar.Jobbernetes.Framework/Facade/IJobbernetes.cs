using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Facade
{
    public interface IJobbernetes
    {
        Task Run(CancellationToken cancellationToken);
    }
}
