using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB.Options;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server
{
    internal static class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureJobberAspNet()
                .ConfigureServices((context, services) =>
                                       services.Configure<MongoOptions>(context.Configuration.GetSection(nameof(MongoOptions))))
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
