using Autofac;
using Confluent.Kafka;
using Google.Protobuf;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    public static class AutofacExtensions
    {
        public static void UseKafkaProtobuf<TData>(this ContainerBuilder builder)
            where TData : IMessage<TData>, new()
        {
            builder.Register(context =>
            {
                var a = new ProducerBuilder<Null, TData>(context.Resolve<ProducerConfig>());
                a.SetValueSerializer(new ProtobufSerializer<TData>());

                return a;
            });

            builder.Register(context =>
            {
                var a = new ConsumerBuilder<Ignore, TData>(context.Resolve<ConsumerConfig>());
                a.SetValueDeserializer(new ProtobufDeserializer<TData>());

                return a;
            });
        }
    }
}
