using System;

namespace Realmar.Jobbernetes.Infrastructure.Messaging.Serialization
{
    /// <summary>
    ///     Serializes data.
    /// </summary>
    public interface ISerializer
    {
        string ContentType { get; }

        ReadOnlyMemory<byte> Serialize<TData>(TData                  data);
        ReadOnlyMemory<byte> Serialize(Type                          type, object data);
        TData                Deserialize<TData>(ReadOnlyMemory<byte> bytes);
        object               Deserialize(Type                        type, ReadOnlyMemory<byte> bytes);
    }
}
