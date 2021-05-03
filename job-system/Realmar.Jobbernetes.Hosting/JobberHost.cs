using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Realmar.Jobbernetes.Framework;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHost
    {
        public static Task StartAsync<TJobbernetesModule, TData>()
            where TJobbernetesModule : AutofacModule<TData>, new() =>
            Host.CreateDefaultBuilder(Environment.GetCommandLineArgs())
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging(ConfigureLogging)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureContainer<ContainerBuilder>(ConfigureContainer<TJobbernetesModule>)
                .RunConsoleAsync();

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConsole();
        }

        private static void ConfigureAppConfiguration(IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();
            builder.AddIniFile("appsettings.ini", optional: true, reloadOnChange: true);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<JobService>();
        }

        private static void ConfigureContainer<TModule>(ContainerBuilder builder)
            where TModule : class, IModule, new()
        {
            builder.RegisterModule<TModule>();
        }
    }
}
