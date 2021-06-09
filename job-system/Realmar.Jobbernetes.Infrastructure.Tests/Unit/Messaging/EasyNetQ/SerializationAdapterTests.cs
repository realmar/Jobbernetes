using System;
using Autofac.Extras.Moq;
using Moq;
using Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ;
using Realmar.Jobbernetes.Infrastructure.Messaging.Serialization;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Messaging.EasyNetQ
{
    public class SerializationAdapterTests
    {
        private static readonly byte[]? Bytes = { 0x02, 0x08 };

        [Fact]
        public void MessageToBytes()
        {
            using var mock = AutoMock.GetLoose();
            mock.Mock<ISerializer>()
                .Setup(serializer => serializer.Serialize(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns(Bytes);

            object message = new();
            var    result  = mock.Create<SerializerAdapter>().MessageToBytes(typeof(string), message);

            Assert.Equal(Bytes, result);
            mock.Mock<ISerializer>().Verify(serializer => serializer.Serialize(typeof(string), message));
        }

        [Fact]
        public void BytesToMessage()
        {
            using var mock = AutoMock.GetLoose();

            mock.Create<SerializerAdapter>().BytesToMessage(typeof(string), Bytes);

            mock.Mock<ISerializer>().Verify(serializer => serializer.Deserialize(typeof(string), new(Bytes)));
        }
    }
}
