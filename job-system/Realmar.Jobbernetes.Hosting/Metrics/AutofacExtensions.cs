using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Options;
using Prometheus.Client;
using Prometheus.Client.Collectors;
using Prometheus.Client.MetricPusher;
using Prometheus.Client.MetricServer;
using Realmar.Jobbernetes.Infrastructure.Options.Metrics;
using MetricServerOptions = Realmar.Jobbernetes.Infrastructure.Options.Metrics.MetricServerOptions;

namespace Realmar.Jobbernetes.Infrastructure.Metrics
{
    public static class AutofacExtensions
    {
        public static void RegisterConsoleMetricServer(this ContainerBuilder builder)
        {
            builder.RegisterMetricsDependencies();
            builder.Register(context =>
            {
                var options  = context.Resolve<IOptions<MetricServerOptions>>().Value;
                var registry = context.Resolve<ICollectorRegistry>();

                return new MetricServer(
                    registry,
                    new()
                    {
                        Host    = options.Hostname,
                        Port    = options.Port,
                        MapPath = options.Path
                    });
            }).As<IMetricServer>();

            builder.RegisterType<AutofacMetricsActivator>()
                   .As<IStartable>()
                   .SingleInstance();
        }

        public static void RegisterMetricsPusher(this ContainerBuilder builder)
        {
            builder.RegisterMetricsDependencies();
            builder.Register(context =>
                    {
                        var options  = context.Resolve<IOptions<MetricPusherOptions>>().Value;
                        var registry = context.Resolve<ICollectorRegistry>();

                        return new MetricPusher(
                            registry,
                            options.Endpoint,
                            options.Job,
                            null,
                            new KeyValuePair<string, string>[] { new("job_name", options.JobName) },
                            null);
                    })
                   .As<IMetricPusher>()
                   .SingleInstance();
        }

        public static void RegisterMetricsNameDecorator(this ContainerBuilder                          builder,
                                                        Func<IMetricsNameFactory, IMetricsNameFactory> factory)
        {
            builder.RegisterDecorator<IMetricsNameFactory>((_, __, instance) => factory.Invoke(instance));
        }

        private static void RegisterMetricsDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<CollectorRegistry>()
                   .As<ICollectorRegistry>()
                   .SingleInstance();

            builder.Register(context => new MetricFactory(context.Resolve<ICollectorRegistry>()))
                   .As<IMetricFactory>()
                   .SingleInstance();
        }
    }
}
