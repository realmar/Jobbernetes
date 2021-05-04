using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Google.Protobuf;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    public class ProtobufDeserializer<T> : IDeserializer<T>
        where T : IMessage<T>, new()
    {
        private readonly MessageParser<T> _parser;

        public ProtobufDeserializer()
        {
            _parser = new(() => new());
        }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) =>
            _parser.ParseFrom(data.ToArray());

        public IEnumerable<KeyValuePair<string, object>>
            Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
            => config;

        public void Dispose() { }
    }
}
