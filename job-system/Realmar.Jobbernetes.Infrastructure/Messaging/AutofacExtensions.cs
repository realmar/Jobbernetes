using Autofac;
using Realmar.Jobbernetes.Framework.Messaging.RabbitMQ;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public static class AutofacExtensions
    {
        public static void UseEndlessConsumer(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(RabbitMQEndlessConsumer<>)).AsImplementedInterfaces();
        }
    }
}
