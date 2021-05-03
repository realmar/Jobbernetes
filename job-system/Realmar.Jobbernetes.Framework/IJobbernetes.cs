using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework
{
    public interface IJobbernetes
    {
        Task Run(CancellationToken cancellationToken);
    }
}
