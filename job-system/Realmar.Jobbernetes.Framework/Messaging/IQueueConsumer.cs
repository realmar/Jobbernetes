using System.Collections.Generic;
using System.Threading;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IQueueConsumer<out TData>
    {
        IAsyncEnumerable<TData> Consume(CancellationToken cancellationToken);
    }
}
