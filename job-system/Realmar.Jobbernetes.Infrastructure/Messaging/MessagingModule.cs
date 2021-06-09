using Autofac;
using Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ;
using Realmar.Jobbernetes.Infrastructure.Messaging.Serialization;

namespace Realmar.Jobbernetes.Infrastructure.Messaging
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.UseJsonSerialization();
            builder.RegisterModule<EasyNetQModule>();
        }
    }
}
