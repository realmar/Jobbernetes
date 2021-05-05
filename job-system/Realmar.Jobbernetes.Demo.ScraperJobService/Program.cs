using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Extensions.Serialization.Kafka;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    internal static class Program
    {
        private static Task Main(string[] args) =>
            JobberHost.RunJobAsync<ImageIngress>(args, builder =>
            {
                builder.UseKafkaJson<ImageIngress>();
                builder.UseKafkaJson<Image>();
                builder.RegisterType<Job>().AsImplementedInterfaces();
                builder.RegisterType<HttpClient>().SingleInstance();
            });
    }
}
