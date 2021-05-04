using Autofac;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Extensions.Serialization.Kafka;
using Realmar.Jobbernetes.Framework;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    public class JobModule : AutofacModule<ImageIngress>
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // builder.RegisterModule<GRPCModule>();
            builder.UseKafkaProtobuf<ImageIngress>();
            builder.UseKafkaProtobuf<Image>();
            builder.RegisterType<Job>().AsImplementedInterfaces();
        }
    }
}
