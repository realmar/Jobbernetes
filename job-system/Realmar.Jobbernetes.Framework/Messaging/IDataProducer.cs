using System.Collections.Generic;
using System.Threading;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public interface IDataProducer<out TData>
    {
        IAsyncEnumerable<TData> Produce(CancellationToken cancellationToken);
    }
}
