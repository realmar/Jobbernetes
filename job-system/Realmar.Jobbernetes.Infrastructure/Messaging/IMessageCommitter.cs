using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IMessageCommitter
    {
        Task CommitAsync(CancellationToken   cancellationToken);
        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
