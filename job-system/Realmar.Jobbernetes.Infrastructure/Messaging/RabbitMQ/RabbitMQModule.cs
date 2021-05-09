using Autofac;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging.RabbitMQ
{
    internal class RabbitMQModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(RabbitMQConsumer<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(RabbitMQProducer<>)).AsImplementedInterfaces();

            builder.RegisterType<RabbitMQMessageCommitter>().AsSelf();
            builder.RegisterType<ChannelProvider>().AsSelf().InstancePerDependency();

            builder.Register(context =>
                    {
                        var options = context.Resolve<IOptions<RabbitMQConnectionOptions>>().Value;

                        var factory = new ConnectionFactory
                        {
                            HostName                 = options.Hostname,
                            UserName                 = options.Username,
                            Password                 = options.Password,
                            DispatchConsumersAsync   = true,
                            AutomaticRecoveryEnabled = true
                        };

                        return factory.CreateConnection();
                    })
                   .SingleInstance()
                   .As<IConnection>();
        }
    }
}
