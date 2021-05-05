using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework.Facade;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHost
    {
        public static Task RunConsoleAsync(string[]                   args,
                                           Action<IServiceCollection> configureServices,
                                           Action<ContainerBuilder>   configureContainer,
                                           Action<IHostBuilder>?      configureHostBuilder = null)
        {
            var host = Host.CreateDefaultBuilder(args)
                           .ConfigureJobberConsoleApp()
                           .ConfigureServices(configureServices)
                           .ConfigureContainer(configureContainer);

            configureHostBuilder?.Invoke(host);

            return host.RunConsoleAsync();
        }

        public static Task RunJobAsync<TData>(string[] args, Action<ContainerBuilder> configureContainer) =>
            RunConsoleAsync(args,
                            services => services.AddHostedService<JobService>(),
                            builder =>
                            {
                                builder.RegisterModule<FacadeModule<TData>>();
                                builder.RegisterModule<JobsModule>();
                                builder.RegisterModule<MessagingModule>();

                                configureContainer.Invoke(builder);
                            });
    }
}
