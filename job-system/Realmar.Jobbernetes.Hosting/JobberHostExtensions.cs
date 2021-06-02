using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus.Client.DependencyInjection;
using Realmar.Jobbernetes.Framework.Facade;
using Realmar.Jobbernetes.Hosting.Logging.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Serilog;
using Serilog.Formatting.Compact;

namespace Realmar.Jobbernetes.Hosting
{
    public static class JobberHostExtensions
    {
        public static IHostBuilder ConfigureJobberConsoleApp(this IHostBuilder builder) => builder.ConfigureCommon();

        public static IHostBuilder ConfigureJobberAspNet(this IHostBuilder builder) => builder.ConfigureCommon();

        private static IHostBuilder ConfigureCommon(this IHostBuilder builder) =>
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                   .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                   .ConfigureAppConfiguration(ConfigureAppConfiguration)
                   .ConfigureServices(ConfigureOptions)
                   .ConfigureServices(ConfigureMetrics)
                   .UseSerilog(ConfigureLogger);

        private static void ConfigureLogger(HostBuilderContext  context,
                                            IServiceProvider    services,
                                            LoggerConfiguration configuration)
        {
            var lokiOptions = services.GetService<IOptions<SerilogOptions>>()!.Value;

            configuration.ReadFrom.Configuration(context.Configuration)
                         .ReadFrom.Services(services)
                         .Enrich.FromLogContext()
                         .WriteTo.Console(new RenderedCompactJsonFormatter());

            // This does not work at the moment
            //
            // Install:
            //      Serilog.Sinks.Loki.gRPC
            //      Grpc.Core
            //
            // .WriteTo.LokigRPC(lokiOptions.Loki.Hostname);
            // .WriteTo.GrafanaLoki(lokiOptions.Loki.Hostname);
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MetricsModule>();
        }

        private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
        {
            config.AddEnvironmentVariables();
        }

        private static void ConfigureOptions(HostBuilderContext context, IServiceCollection services)
        {
            void Configure<TOptions>(string? sectionName = null) where TOptions : class =>
                services.Configure<TOptions>(context.Configuration.GetSection(sectionName ?? typeof(TOptions).Name));

            var options = typeof(IJobbernetes).Assembly.GetTypes()
                                              .Where(type =>
                                                         type!.Namespace!.StartsWith("Realmar.Jobbernetes.Framework.Options") &&
                                                         type!.IsAbstract == false)
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

            Configure<SerilogOptions>(SerilogOptions.Position);

            services.Configure<HostOptions>(hostOptions => hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(120));
        }

        private static void ConfigureMetrics(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMetricFactory();
        }
    }
}
