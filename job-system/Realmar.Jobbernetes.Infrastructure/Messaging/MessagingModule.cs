using Autofac;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging.EasyNetQ;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.UseJsonSerialization();

            builder.RegisterModule<EasyNetQModule>();
            builder.RegisterType<NullQueueConsumer>().As<IQueueConsumer<NullInput>>();
        }
    }
}
