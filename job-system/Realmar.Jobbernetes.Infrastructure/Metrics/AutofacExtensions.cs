using System;
using Autofac;
using Microsoft.Extensions.Options;
using Prometheus;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public static class AutofacExtensions
    {
        public static void RegisterConsoleMetricServer(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<MetricServerOptions>>().Value;
                return new MetricServer(hostname: options.Hostname, port: options.Port);
            }).As<IMetricServer>();
        }

        public static void RegisterMetricsPusher(this ContainerBuilder builder)
        {
            builder.Register(context =>
                    {
                        var options = context.Resolve<IOptions<MetricPusherOptions>>().Value;

                        return new MetricPusher(new()
                        {
                            Endpoint = options.Endpoint ?? "http://localhost:9091/metrics",
                            Job      = options.Job      ?? "some_job"
                        });
                    })
                   .As<IMetricServer>()
                   .SingleInstance();

            builder.RegisterType<AutofacMetricsActivator>()
                   .As<IStartable>()
                   .SingleInstance();
        }

        public static void RegisterMetricsNameDecorator(this ContainerBuilder                          builder,
                                                        Func<IMetricsNameFactory, IMetricsNameFactory> factory)
        {
            builder.RegisterDecorator<IMetricsNameFactory>((_, __, instance) => factory.Invoke(instance));
        }
    }
}
