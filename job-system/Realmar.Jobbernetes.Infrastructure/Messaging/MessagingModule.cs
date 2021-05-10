using Autofac;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging.EasyNetQ;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<EasyNetQModule>();
            builder.RegisterType<NullQueueConsumer>().As<IQueueConsumer<NullInput>>();
        }
    }
}
