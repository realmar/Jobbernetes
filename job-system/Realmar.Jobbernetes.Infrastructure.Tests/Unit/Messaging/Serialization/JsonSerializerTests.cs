using System;
using System.Text;
using Autofac.Extras.Moq;
using Realmar.Jobbernetes.Infrastructure.Messaging.Serialization;
using Xunit;

namespace Realmar.Jobbernetes.Infrastructure.Tests.Unit.Messaging.Serialization
{
    public class JsonSerializerTests : IDisposable
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly AutoMock       _mock;

        public JsonSerializerTests()
        {
            _mock           = AutoMock.GetLoose(builder => builder.UseJsonSerialization());
            _jsonSerializer = _mock.Create<JsonSerializer>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _mock.Dispose();
        }

        [Fact]
        public void Serialize()
        {
            // Arrange
            var data = TestData.Create();

            // Act
            var bytes = _jsonSerializer.Serialize(typeof(TestData), data);
            var json  = Encoding.UTF8.GetString(bytes.Span);

            // Assert
            Assert.Contains(data.Id.ToString(), json);
            Assert.Contains(data.Name,          json);
        }

        [Fact]
        public void SerializeGeneric() { }

        [Fact]
        public void Deserialize()
        {
            // Arrange
            var source = TestData.Create();
            var json   = "{ \"Id\": " + source.Id + ", \"Name\": \"" + source.Name + "\" }";

            // Act
            var data = _jsonSerializer.Deserialize(typeof(TestData), new(Encoding.UTF8.GetBytes(json)));

            // Assert
            Assert.Equal(source, data);
        }

        [Fact]
        public void DeserializeGeneric() { }

        private sealed class TestData
        {
            public int    Id   { get; set; }
            public string Name { get; set; }

            private bool Equals(TestData other) => Id == other.Id && Name == other.Name;

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((TestData) obj);
            }

            public override int GetHashCode() => HashCode.Combine(Id, Name);

            public static TestData Create() => new()
            {
                Id   = 28,
                Name = "Hello World"
            };
        }
    }
}
