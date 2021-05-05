using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    internal class JsonSerializer<TData> : ISerializer<TData>
    {
        public byte[] Serialize(TData data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }
    }
}
