using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Realmar.Jobbernetes.Demo.Infrastructure.Options;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.Demo.ImageScrapeJob
{
    internal static class Program
    {
        private static Task Main(string[] args) => JobberHost.RunJobAsync<ImageInput>(
            args,
            (context, services) =>
            {
                services.Configure<ExternalServiceOptions>(context.Configuration.GetSection(nameof(ExternalServiceOptions)));
                services.Configure<DemoOptions>(context.Configuration.GetSection(nameof(DemoOptions)));
            },
            builder =>
            {
                builder.RegisterType<Job>().AsImplementedInterfaces();
                builder.RegisterType<HttpClient>().SingleInstance();
            });
    }
}
