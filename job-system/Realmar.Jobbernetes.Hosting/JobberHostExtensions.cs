using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                   .ConfigureAppConfiguration(ConfigureAppConfiguration);

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.AddConsole();
        }

        private static void ConfigureAppConfiguration(IConfigurationBuilder builder)
        {
            builder.AddEnvironmentVariables();
            builder.AddIniFile("appsettings.ini", optional: true, reloadOnChange: true);
        }
    }
}
