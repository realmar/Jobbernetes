using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Demo.Utilities.Serialization.MongoDB;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BsonClassMapper.MapProtobufModels();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
