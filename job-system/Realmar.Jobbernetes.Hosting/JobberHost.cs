using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework.Facade;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHost
    {
        public static Task RunConsoleAsync(string[]                                       args,
                                           Action<HostBuilderContext, IServiceCollection> configureServices,
                                           Action<ContainerBuilder>                       configureContainer,
                                           Action<IHostBuilder>?                          configureHostBuilder = null)
        {
            var host = Host.CreateDefaultBuilder(args)
                           .ConfigureJobberConsoleApp()
                           .ConfigureServices(configureServices)
                           .ConfigureContainer(configureContainer);

            configureHostBuilder?.Invoke(host);

            return host.RunConsoleAsync();
        }

        public static Task RunJobAsync<TData>(string[] args,
                                              Action<HostBuilderContext, IServiceCollection>? configureServices = null,
                                              Action<ContainerBuilder>? configureContainer = null) => RunConsoleAsync(args,
            (context, services) =>
            {
                services.AddHostedService<JobService>();
                configureServices?.Invoke(context, services);
            },
            builder =>
            {
                builder.RegisterModule<FacadeModule<TData>>();
                builder.RegisterModule<JobsModule>();
                builder.RegisterModule<MessagingModule>();

                builder.RegisterMetricsNameDecorator(factory => new PrefixMetricsNameFactory("job", factory));
                builder.RegisterMetricsPusher();

                configureContainer?.Invoke(builder);
            });
    }
}
