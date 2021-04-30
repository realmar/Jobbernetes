using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework;

namespace Realmar.Jobbernetes.Hosting
{
    public abstract class JobService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IJobbernetes             _jobbernetes;

        protected JobService(IJobbernetes jobbernetes, IHostApplicationLifetime application)
        {
            _jobbernetes = jobbernetes;
            _application = application;
        }

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunAsync(stoppingToken).ConfigureAwait(false);
            _application.StopApplication();
        }

        protected abstract Task RunAsync(CancellationToken cancellationToken);
    }
}
