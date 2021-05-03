using Autofac;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Framework;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    public class JobModule : AutofacModule<ImageIngress>
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<Job>().AsImplementedInterfaces();
        }
    }
}
