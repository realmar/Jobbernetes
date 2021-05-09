using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Messaging.Serialization
{
    public interface IDeserializer<TData>
    {
        Task<TData?> DeserializeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
    }
}
