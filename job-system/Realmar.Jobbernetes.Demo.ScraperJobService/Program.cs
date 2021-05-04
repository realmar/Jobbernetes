using System.Threading.Tasks;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    internal static class Program
    {
        private static Task Main()
        {
            return JobberHost.StartAsync<JobModule, ImageIngress>();
        }
    }
}
