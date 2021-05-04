using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework.Facade;

namespace Realmar.Jobbernetes.Hosting
{
    public class JobService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IJobbernetes             _jobbernetes;

        public JobService(IJobbernetes jobbernetes, IHostApplicationLifetime application)
        {
            _jobbernetes = jobbernetes;
            _application = application;
        }

        protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _jobbernetes.Run(cancellationToken).ConfigureAwait(false);
            _application.StopApplication();
        }
    }
}
