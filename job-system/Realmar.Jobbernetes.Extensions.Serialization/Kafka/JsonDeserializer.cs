using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    public class JsonDeserializer<TData> : IAsyncDeserializer<TData>
    {
        public async Task<TData> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)
        {
            await using var stream = new MemoryStream(data.ToArray());
            var             result = await JsonSerializer.DeserializeAsync<TData>(stream).ConfigureAwait(false);
            stream.ConfigureAwait(false);

            return result;
        }
    }
}
