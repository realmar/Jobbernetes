using Autofac;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public class MetricsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MetricsNameFactory>()
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<SuffixMetricsNameFactory>();
        }
    }
}
