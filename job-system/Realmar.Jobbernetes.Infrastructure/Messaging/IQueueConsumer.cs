using System.Collections.Generic;
using System.Threading;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IQueueConsumer<TData>
    {
        IAsyncEnumerable<Message<TData>> ConsumeAsync(CancellationToken cancellationToken);
    }
}
