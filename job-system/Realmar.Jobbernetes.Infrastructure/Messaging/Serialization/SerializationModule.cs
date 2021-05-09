using Autofac;

namespace Realmar.Jobbernetes.Framework.Messaging.Serialization
{
    internal class SerializationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(JsonSerializer<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(JsonDeserializer<>)).AsImplementedInterfaces();
        }
    }
}
