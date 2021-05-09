using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging.Serialization
{
    public interface ISerializer<TData>
    {
        public Task<ReadOnlyMemory<byte>> SerializeAsync(TData data, CancellationToken cancellationToken);
    }
}
