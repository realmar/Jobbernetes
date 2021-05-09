using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Realmar.Jobbernetes.Framework.Facade;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHostExtensions
    {
        public static IHostBuilder ConfigureJobberConsoleApp(this IHostBuilder builder) =>
            builder.ConfigureCommon();

        public static IHostBuilder ConfigureJobberAspNet(this IHostBuilder builder) =>
            builder.ConfigureCommon();

        private static IHostBuilder ConfigureCommon(this IHostBuilder builder) =>
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                   .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                   .ConfigureLogging(ConfigureLogging)
                   .ConfigureAppConfiguration(ConfigureAppConfiguration)
                   .ConfigureServices(ConfigureServices);

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MetricsModule>();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConsole();
        }

        private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
        {
            config.AddEnvironmentVariables();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            void Configure<TOptions>() where TOptions : class =>
                services.Configure<TOptions>(context.Configuration.GetSection(typeof(TOptions).Name));

            var options = typeof(IJobbernetes).Assembly.GetTypes()
                                              .Where(type => type.Namespace == "Realmar.Jobbernetes.Framework.Options")
                                              .ToArray();

            var           extensionsType = typeof(OptionsConfigurationServiceCollectionExtensions);
            const string? configureName  = nameof(OptionsConfigurationServiceCollectionExtensions.Configure);

            var registerMethod = extensionsType.GetMethod(
                configureName,
                BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(IServiceCollection), typeof(IConfiguration) }, Array.Empty<ParameterModifier>());

            if (registerMethod == null)
            {
                throw new InvalidOperationException($"Cannot find method {configureName} on type {extensionsType.FullName}. " +
                                                    "This is a bug in the library.");
            }

            foreach (var option in options)
            {
                registerMethod.MakeGenericMethod(option)
                              .Invoke(null, new object?[] { services, context.Configuration.GetSection(option.Name) });
            }

            Configure<MetricPusherOptions>();
        }
    }
}
