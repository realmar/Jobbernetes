using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using Realmar.Jobbernetes.AdminWeb.Client.Formatters;

namespace Realmar.Jobbernetes.AdminWeb.Client
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.ConfigureContainer(new AutofacServiceProviderFactory(ConfigureContainer));

            builder.RootComponents.Add<App>("#app");
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            builder.Services.AddMudServices();

            return builder.Build().RunAsync();
        }

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register(context => new HttpClient
            {
                BaseAddress = new(context.Resolve<IWebAssemblyHostEnvironment>().BaseAddress)
            }).InstancePerLifetimeScope();

            builder.RegisterType<TaskDisposer>().InstancePerDependency();

            builder.RegisterType<HubFactory>();

            builder.RegisterType<ColorProvider>();

            builder.Register(_ => new MudTheme() /*new MudTheme
            {
                Palette = new()
                {
                    Black                    = "#27272f",
                    Background               = "#32333d",
                    BackgroundGrey           = "#27272f",
                    Surface                  = "#373740",
                    DrawerBackground         = "#27272f",
                    DrawerText               = "rgba(255,255,255, 0.50)",
                    DrawerIcon               = "rgba(255,255,255, 0.50)",
                    AppbarBackground         = "#27272f",
                    AppbarText               = "rgba(255,255,255, 0.70)",
                    TextPrimary              = "rgba(255,255,255, 0.70)",
                    TextSecondary            = "rgba(255,255,255, 0.50)",
                    ActionDefault            = "#adadb1",
                    ActionDisabled           = "rgba(255,255,255, 0.26)",
                    ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                    Divider                  = "rgba(255,255,255, 0.12)",
                    DividerLight             = "rgba(255,255,255, 0.06)",
                    TableLines               = "rgba(255,255,255, 0.12)",
                    LinesDefault             = "rgba(255,255,255, 0.12)",
                    LinesInputs              = "rgba(255,255,255, 0.3)",
                    TextDisabled             = "rgba(255,255,255, 0.2)"
                }
            }*/);
        }
    }
}
