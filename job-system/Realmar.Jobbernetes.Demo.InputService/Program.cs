using System.Threading.Tasks;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.Demo.InputService
{
    internal static class Program
    {
        public static Task Main(string[] args) =>
            JobberHost.RunAspNetAsync<Startup>(args);
    }
}
