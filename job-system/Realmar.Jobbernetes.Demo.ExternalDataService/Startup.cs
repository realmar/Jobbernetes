using System.IO;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using SixLabors.Fonts;

namespace Realmar.Jobbernetes.Demo.ExternalImageService
{
    internal class Startup
    {
        private readonly IHostEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            Configuration       = configuration;
            _hostingEnvironment = hostingEnvironment;
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
                    new() { Title = "Realmar.Jobbernetes.Demo.ExternalImageService", Version = "v1" });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MessagingModule>();

            builder.Register(context =>
            {
                const int fontSize = 28;
                FontCollection fonts = new();
                var font = fonts.Install(Path.Combine(_hostingEnvironment.ContentRootPath, "Fonts/JetBrainsMono-Regular.ttf"));

                return font.CreateFont(fontSize);
            }).As<Font>();

            builder.RegisterMetricsNameDecorator(factory => new SuffixMetricsNameFactory("external_data_service", factory));
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
                                                    "Realmar.Jobbernetes.Demo.ExternalImageService v1"));

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
