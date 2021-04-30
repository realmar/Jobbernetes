using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHost
    {
        public static Task StartAsync<TJobService>()
            where TJobService : class, IHostedService => Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
                                                             .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                                                             .ConfigureAppConfiguration(ConfigureAppConfiguration)
                                                             .ConfigureServices(ConfigureServices<TJobService>)
                                                             .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                                                             .RunConsoleAsync();

        private static void ConfigureAppConfiguration(IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();
            builder.AddIniFile("appsettings.ini", optional: true, reloadOnChange: true);
        }

        private static void ConfigureServices<TJobService>(IServiceCollection services)
            where TJobService : class, IHostedService
        {
            services.AddHostedService<TJobService>();
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder!.RegisterModule<AutofacModule>();
        }
    }
}
