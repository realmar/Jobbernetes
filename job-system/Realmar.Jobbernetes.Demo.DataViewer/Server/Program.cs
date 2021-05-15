using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB.Options;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server
{
    internal static class Program
    {
        public static Task Main(string[] args) =>
            JobberHost.RunAspNetAsync<Startup>(args,
                                               (context, services) =>
                                                   services.Configure<MongoOptions>(
                                                       context.Configuration.GetSection(nameof(MongoOptions))));
    }
}
