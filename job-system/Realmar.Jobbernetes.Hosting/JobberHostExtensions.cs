using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Realmar.Jobbernetes.Framework.Options;

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
                   .ConfigureLogging(ConfigureLogging)
                   .ConfigureAppConfiguration(ConfigureDelegate)
                   .ConfigureServices(ConfigureDelegate);

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConsole();
        }

        private static void ConfigureDelegate(HostBuilderContext context, IConfigurationBuilder config)
        {
            config.AddEnvironmentVariables();
        }

        private static void ConfigureDelegate(HostBuilderContext context, IServiceCollection services)
        {
            void Configure<TOptions>() where TOptions : class =>
                services.Configure<TOptions>(context.Configuration.GetSection(typeof(TOptions).Name));

            Configure<KafkaOptions>();
            Configure<KafkaProducerOptions>();
            Configure<KafkaConsumerOptions>();
            Configure<ProcessingOptions>();
        }
    }
}
