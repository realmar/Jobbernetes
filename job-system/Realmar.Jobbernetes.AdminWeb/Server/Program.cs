using System.Threading.Tasks;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.AdminWeb.Server
{
    public class Program
    {
        public static Task Main(string[] args)
            => JobberHost.RunAspNetAsync<Startup>(args);
    }
}
