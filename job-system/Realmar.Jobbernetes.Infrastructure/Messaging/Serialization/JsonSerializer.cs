using System;
using System.Text;
using System.Text.Json;

namespace Realmar.Jobbernetes.Infrastructure.Messaging.Serialization
{
    internal class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer(JsonSerializerOptions options) => _options = options;

        public string ContentType { get; } = "application/json";

        public ReadOnlyMemory<byte> Serialize<TData>(TData data) =>
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);

        public ReadOnlyMemory<byte> Serialize(Type type, object data) =>
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data, _options);

        public TData Deserialize<TData>(ReadOnlyMemory<byte> bytes) =>
            Deserialize(bytes, bytes => System.Text.Json.JsonSerializer.Deserialize<TData>(bytes.Span, _options));

        public object Deserialize(Type type, ReadOnlyMemory<byte> bytes) =>
            Deserialize(bytes, bytes => System.Text.Json.JsonSerializer.Deserialize(bytes.Span, type, _options));

        private static TData Deserialize<TData>(ReadOnlyMemory<byte> bytes, Func<ReadOnlyMemory<byte>, TData?> serializer)
        {
            try
            {
                var data = serializer.Invoke(bytes);

                if (data == null)
                {
                    var e = new JsonException("Deserialized JSON is null");
                    EnrichExceptionWithData(bytes, e);

                    throw e;
                }

                return data;
            }
            catch (Exception e) when (e is ArgumentException || e is JsonException || e is NotSupportedException)
            {
                EnrichExceptionWithData(bytes, e);
                throw;
            }
        }

        private static void EnrichExceptionWithData(ReadOnlyMemory<byte> bytes, Exception e)
        {
            if (e.Data.IsReadOnly == false)
            {
                var json = "<Cannot convert byte array to UTF8 string>";

                try
                {
                    json = Encoding.UTF8.GetString(bytes.Span);
                }
                catch
                {
                    // Nothing to do
                    // We tell the caller, that there was an error
                    // in decoding the byte array to an UTF8 string
                }

                e.Data.Add(Constants.StringDataKey, json);
            }
        }
    }
}
