using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus.Client.AspNetCore;
using Prometheus.Client.HttpRequestDurations;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

#pragma warning disable CA1822 // Mark members as static
namespace Realmar.Jobbernetes.Demo.InputService
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
                        Title   = "Realmar.Jobbernetes.Demo.InputService",
                        Version = "v1"
                    });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MessagingModule>();
            builder.RegisterMetricsNameDecorator(factory => new PrefixMetricsNameFactory("input", factory));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Realmar.Jobbernetes.Demo.InputService v1"));

            app.UseRouting();

            app.UsePrometheusServer();
            app.UsePrometheusRequestDurations();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
#pragma warning restore CA1822 // Mark members as static
