using System.Collections.Generic;
using Confluent.Kafka;
using Google.Protobuf;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    public class ProtobufSerializer<T> : ISerializer<T> where T : IMessage<T>
    {
        public byte[] Serialize(T data, SerializationContext context) =>
            data.ToByteArray();

        public IEnumerable<KeyValuePair<string, object>>
            Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
            => config;

        public void Dispose() { }
    }
}
