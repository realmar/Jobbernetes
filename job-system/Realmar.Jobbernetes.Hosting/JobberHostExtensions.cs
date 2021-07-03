using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Prometheus.Client.DependencyInjection;
using Realmar.Jobbernetes.Hosting.Logging.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Realmar.Jobbernetes.Infrastructure.Options;
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
            var configuration = context.Configuration;

            services.ConfigureAllOptions(configuration);
            services.Configure<SerilogOptions>(configuration, SerilogOptions.Position);
            services.Configure<HostOptions>(hostOptions => hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(120));
        }

        private static void ConfigureMetrics(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMetricFactory();
        }
    }
}
