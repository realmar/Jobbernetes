using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Realmar.Jobbernetes.Framework.Messaging.Serialization
{
    public class JsonDeserializer<TData> : IDeserializer<TData>
    {
        private readonly ILogger<JsonDeserializer<TData>> _logger;

        public JsonDeserializer(ILogger<JsonDeserializer<TData>> logger) => _logger = logger;

        public async Task<TData?> DeserializeAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var             stream = new MemoryStream(data.ToArray());
            await using var _      = stream.ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Deserializing: {Encoding.UTF8.GetString(data.ToArray())}");
            }

            return await JsonSerializer.DeserializeAsync<TData>(stream, cancellationToken: cancellationToken)
                                       .ConfigureAwait(false);
        }
    }
}
