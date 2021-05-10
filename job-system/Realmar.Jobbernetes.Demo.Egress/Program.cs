using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB.Options;
using Realmar.Jobbernetes.Demo.Models;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Hosting;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Demo.Egress
{
    internal static class Program
    {
        private static Task Main(string[] args)
        {
            return JobberHost.RunConsoleAsync(
                args,
                (context, services) =>
                {
                    services.AddHostedService<EgressService>();
                    services.Configure<MongoOptions>(
                        context.Configuration.GetSection(nameof(MongoOptions)));
                },
                builder =>
                {
                    builder.RegisterModule<MongoDBModule<Image>>();
                    builder.RegisterModule<MessagingModule>();

                    builder.RegisterMetricsNameDecorator(factory => new SuffixMetricsNameFactory("egress", factory));
                    builder.RegisterConsoleMetricServer();
                });
        }
    }
}
