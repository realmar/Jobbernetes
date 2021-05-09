using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Realmar.Jobbernetes.Framework.Messaging.Serialization
{
    public class JsonSerializer<TData> : ISerializer<TData>
    {
        private readonly ILogger<JsonSerializer<TData>> _logger;

        public JsonSerializer(ILogger<JsonSerializer<TData>> logger) => _logger = logger;

        public async Task<ReadOnlyMemory<byte>> SerializeAsync(TData data, CancellationToken cancellationToken)
        {
            var             stream = new MemoryStream();
            await using var _      = stream.ConfigureAwait(false);

            await JsonSerializer.SerializeAsync(stream, data, cancellationToken: cancellationToken).ConfigureAwait(false);

            var bytes = stream.ToArray();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Serialized: {Encoding.UTF8.GetString(bytes)}");
            }

            return new(bytes);
        }
    }
}
