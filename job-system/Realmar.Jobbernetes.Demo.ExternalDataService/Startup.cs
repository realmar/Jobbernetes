using System.IO;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.HttpRequestDurations;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using SkiaSharp;

#pragma warning disable CA1822 // Mark members as static
namespace Realmar.Jobbernetes.Demo.ExternalDataService
{
    internal class Startup
    {
        private readonly IHostEnvironment _environment;

        public Startup(IHostEnvironment environment, IConfiguration configuration)
        {
            Configuration = configuration;
            _environment  = environment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new()
                    {
                        Title   = "Realmar.Jobbernetes.Demo.ExternalDataService",
                        Version = "v1"
                    });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MessagingModule>();
            builder.RegisterMetricsNameDecorator(factory => new PrefixMetricsNameFactory("external_data_service", factory));
            builder.Register(_ => SKTypeface.FromFile(Path.Combine(
                                                          _environment.ContentRootPath,
                                                          "Fonts",
                                                          "JetBrainsMono-Regular.ttf")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
                                                    "Realmar.Jobbernetes.Demo.ExternalDataService v1"));

            app.UseRouting();

            app.UsePrometheusServer();
            app.UsePrometheusRequestDurations();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
#pragma warning restore CA1822 // Mark members as static
