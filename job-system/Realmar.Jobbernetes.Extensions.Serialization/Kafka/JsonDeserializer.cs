using System;
using System.Text.Json;
using Confluent.Kafka;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    internal class JsonDeserializer<TData> : IDeserializer<TData>
    {
        public TData Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<TData>(data);
        }
    }
}
