using System.Text.Json;
using Autofac;

namespace Realmar.Jobbernetes.Infrastructure.Messaging.Serialization
{
    public static class AutofacExtensions
    {
        public static void UseJsonSerialization(this ContainerBuilder builder)
        {
            builder.RegisterType<JsonSerializer>().AsImplementedInterfaces();
            builder.Register(_ => new JsonSerializerOptions
            {
                IgnoreNullValues            = false,
                IgnoreReadOnlyFields        = true,
                IgnoreReadOnlyProperties    = true,
                WriteIndented               = false,
                IncludeFields               = false,
                PropertyNameCaseInsensitive = false
            });
        }
    }
}
