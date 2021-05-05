using Autofac;
using Confluent.Kafka;

namespace Realmar.Jobbernetes.Extensions.Serialization.Kafka
{
    public static class AutofacExtensions
    {
        public static void UseKafkaJson<TData>(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var pb = new ProducerBuilder<Null, TData>(context.Resolve<ProducerConfig>());
                pb.SetValueSerializer(new JsonSerializer<TData>());

                return pb;
            });

            builder.Register(context =>
            {
                var cb = new ConsumerBuilder<Ignore, TData>(context.Resolve<ConsumerConfig>());
                cb.SetValueDeserializer(new JsonDeserializer<TData>());

                return cb;
            });
        }
    }
}
