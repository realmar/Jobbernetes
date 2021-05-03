using System;
using System.Linq;
using Autofac;
using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Framework.Options;
using Realmar.Jobbernetes.Utilities.Serialization.Kafka;

namespace Realmar.Jobbernetes.Framework
{
    public abstract class AutofacModule<TData> : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Jobbernetes<TData>>().AsImplementedInterfaces();
            builder.RegisterType<KafkaDataProducer<TData>>().AsImplementedInterfaces();
            builder.RegisterType<JobDispatcher<TData>>().AsImplementedInterfaces();
            builder.RegisterType<KafkaDataSender<TData>>().AsImplementedInterfaces();

            builder.Register(context =>
                    {
                        var options = context.Resolve<IOptions<KafkaOptions>>().Value;
                        var config  = new ProducerConfig { BootstrapServers = options.BootStrapServers };

                        var builder = new ProducerBuilder<Null, TData>(config);
                        ConfigureSerializer<ISerializer<TData>>(typeof(ProtobufSerializer<>),
                                                                serializer => builder.SetValueSerializer(serializer));

                        return builder.Build();
                    })
                   .As<IProducer<Null, TData>>();

            builder.Register(context =>
                    {
                        var options = context.Resolve<IOptions<KafkaOptions>>().Value;
                        var config = new ConsumerConfig
                        {
                            GroupId          = options.GroupId,
                            BootstrapServers = options.BootStrapServers,

                            // Note: The AutoOffsetReset property determines the start offset in the event
                            // there are not yet any committed offsets for the consumer group for the
                            // topic/partitions of interest. By default, offsets are committed
                            // automatically, so in this example, consumption will only start from the
                            // earliest message in the topic 'my-topic' the first time you run the program.
                            AutoOffsetReset = AutoOffsetReset.Earliest
                        };

                        var builder = new ConsumerBuilder<Ignore, TData>(config);

                        ConfigureSerializer<IDeserializer<TData>>(typeof(ProtobufDeserializer<>),
                                                                  deserializer => builder.SetValueDeserializer(deserializer));

                        return builder.Build();
                    })
                   .As<IConsumer<Ignore, TData>>();
        }

        private void ConfigureSerializer<TInterface>(Type openType, Action<TInterface> setter)
        {
            var isProtobuf = typeof(TData).GetInterfaces()
                                          .Any(x => x.IsGenericType &&
                                                    x.GetGenericTypeDefinition() == typeof(IMessage<>));

            if (isProtobuf)
            {
                var materializedType = openType.MakeGenericType(typeof(TData));
                var deserializer     = (TInterface) Activator.CreateInstance(materializedType);
                setter.Invoke(deserializer);
            }
        }
    }
}
