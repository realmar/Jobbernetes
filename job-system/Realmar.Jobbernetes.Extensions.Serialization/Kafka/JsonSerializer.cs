using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    public class JsonSerializer<TData> : IAsyncSerializer<TData>
    {
        public async Task<byte[]> SerializeAsync(TData data, SerializationContext context)
        {
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, data).ConfigureAwait(false);
            stream.ConfigureAwait(false);

            return stream.ToArray();
        }
    }
}
