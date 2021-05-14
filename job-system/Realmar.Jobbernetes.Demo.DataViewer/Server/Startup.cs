using Autofac;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.HttpRequestDurations;
using Realmar.Jobbernetes.Demo.Infrastructure.MongoDB;
using Realmar.Jobbernetes.Demo.Models;

#pragma warning disable CA1822 // Mark members as static
namespace Realmar.Jobbernetes.Demo.DataViewer.Server
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        [UsedImplicitly]
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MongoDBModule<Image>>();

            var pack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("My Solution Conventions", pack, _ => true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UsePrometheusServer();
            app.UsePrometheusRequestDurations();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
#pragma warning restore CA1822 // Mark members as static
