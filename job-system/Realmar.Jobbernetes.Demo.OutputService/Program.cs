using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB.Options;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Hosting;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.OutputService
{
    internal static class Program
    {
        private static Task Main(string[] args)
        {
            return JobberHost.RunConsoleAsync(
                args,
                (context, services) =>
                {
                    services.AddHostedService<OutputService>();
                    services.Configure<MongoOptions>(
                        context.Configuration.GetSection(nameof(MongoOptions)));
                },
                builder =>
                {
                    builder.RegisterModule<MongoDBModule<ImageOutput>>();
                    builder.RegisterModule<MessagingModule>();

                    builder.RegisterMetricsNameDecorator(factory => new PrefixMetricsNameFactory("output", factory));
                    builder.RegisterConsoleMetricServer();
                });
        }
    }
}
