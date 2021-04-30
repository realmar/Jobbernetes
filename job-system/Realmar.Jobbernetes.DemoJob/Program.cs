using System.Threading.Tasks;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.DemoJob
{
    internal static class Program
    {
        public static Task Main() => JobberHost.StartAsync<DemoJobService>()!;
    }
}
