using System.Collections.Generic;
using Autofac;
using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Infrastructure.Options.RabbitMQ;

namespace Realmar.Jobbernetes.Infrastructure.Messaging.EasyNetQ
{
    internal class EasyNetQModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EasyNetQConsumer<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(EasyNetQBatchConsumer<>)).As(typeof(IQueueBatchConsumer<>));
            builder.RegisterGeneric(typeof(EasyNetQProducer<>)).AsImplementedInterfaces();

            builder.RegisterEasyNetQ(resolver =>
                                     {
                                         var options = resolver.Resolve<IOptions<RabbitMQConnectionOptions>>().Value;
                                         return new()
                                         {
                                             PersistentMessages = true,
                                             UserName           = options.Username,
                                             Password           = options.Password,
                                             PrefetchCount      = 1,
                                             PublisherConfirms  = true,
                                             Hosts = new List<HostConfiguration>
                                             {
                                                 new()
                                                 {
                                                     Host = options.Hostname,
                                                     Ssl  = { Enabled = false },
                                                     Port = (ushort) options.Port
                                                 }
                                             }
                                         };
                                     },
                                     register =>
                                     {
                                         register.Register<ISerializer, SerializerAdapter>();
                                         register.Register<IConsumerErrorStrategy, ConsumerErrorStrategy>();
                                     });
        }
    }
}
