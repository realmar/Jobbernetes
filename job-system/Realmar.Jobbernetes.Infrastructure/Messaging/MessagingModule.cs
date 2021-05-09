using Autofac;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging.RabbitMQ;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<SerializationModule>();
            builder.RegisterModule<RabbitMQModule>();

            builder.RegisterType<NullQueueConsumer>().As<IQueueConsumer<NullInput>>();
        }
    }
}
