using System;
using EasyNetQ;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    public class SerializerAdapter : ISerializer
    {
        private readonly Serialization.ISerializer _serializer;

        public SerializerAdapter(Serialization.ISerializer serializer) => _serializer = serializer;

        public byte[] MessageToBytes(Type messageType, object message) =>
            _serializer.Serialize(messageType, message).ToArray();

        public object BytesToMessage(Type messageType, byte[] bytes) =>
            _serializer.Deserialize(messageType, new(bytes));
    }
}
