using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Realmar.Jobbernetes.AdminWeb.Client.Formatters
{
    internal class HubFactory
    {
        private readonly NavigationManager _navigationManager;

        public HubFactory(NavigationManager navigationManager) => _navigationManager = navigationManager;

        public HubConnection Create(string url)
        {
            return new HubConnectionBuilder()
                  .WithUrl(_navigationManager.ToAbsoluteUri("/overviewhub"))
                  .AddMessagePackProtocol()
                  .Build();
        }
    }
}
