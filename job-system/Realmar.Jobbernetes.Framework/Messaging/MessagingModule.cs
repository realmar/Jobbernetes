using System;
using Autofac;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(KafkaQueueConsumer<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(KafkaQueueProducer<>)).AsImplementedInterfaces();

            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<KafkaOptions>>().Value;
                return new ProducerConfig { BootstrapServers = options.BootStrapServers };
            });

            RegisterGenericKafka(builder, typeof(ProducerBuilder<,>), typeof(ProducerConfig), typeof(IProducer<,>));

            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<KafkaOptions>>().Value;
                return new ConsumerConfig
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
            });

            RegisterGenericKafka(builder, typeof(ConsumerBuilder<,>), typeof(ConsumerConfig), typeof(IConsumer<,>));
        }

        private void RegisterGenericKafka(ContainerBuilder builder, Type builderType, Type configType, Type targetType)
        {
            builder.RegisterGeneric(builderType)
                   .WithParameter((_, _) => true,
                                  (_, context) => context.Resolve(configType));

            builder.RegisterGeneric((context, types) =>
            {
                var builder = context.Resolve(builderType.MakeGenericType(types));
                return builder.GetType()
                              .GetMethod("Build")
                              .Invoke(builder, Array.Empty<object>());
            }).As(targetType);
        }
    }
}
