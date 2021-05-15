using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework.Facade;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Hosting.Logging;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Serilog;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHost
    {
        public static Task RunAspNetAsync<TStartup>(string[]                                        args,
                                                    Action<HostBuilderContext, IServiceCollection>? configureServices = null)
            where TStartup : class
        {
            LoggingBootstrapConfigurator.Configure();
            var builder = Host.CreateDefaultBuilder(args)
                              .ConfigureJobberAspNet()
                              .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TStartup>());

            if (configureServices != null)
            {
                builder.ConfigureServices(configureServices);
            }

            return RunAsync(builder.Build().RunAsync());
        }

        public static Task RunConsoleAsync(string[]                                       args,
                                           Action<HostBuilderContext, IServiceCollection> configureServices,
                                           Action<ContainerBuilder>                       configureContainer,
                                           Action<IHostBuilder>?                          configureHostBuilder = null)
        {
            LoggingBootstrapConfigurator.Configure();
            var host = Host.CreateDefaultBuilder(args)
                           .ConfigureJobberConsoleApp()
                           .ConfigureServices(configureServices)
                           .ConfigureContainer(configureContainer);

            configureHostBuilder?.Invoke(host);

            return RunAsync(host.RunConsoleAsync());
        }

        public static Task RunJobAsync<TData>(string[]                                        args,
                                              Action<HostBuilderContext, IServiceCollection>? configureServices  = null,
                                              Action<ContainerBuilder>?                       configureContainer = null)
        {
            return RunConsoleAsync(args,
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

                                       builder.RegisterMetricsNameDecorator(
                                           factory => new PrefixMetricsNameFactory("job", factory));
                                       builder.RegisterMetricsPusher();

                                       configureContainer?.Invoke(builder);
                                   });
        }

        private static async Task RunAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
