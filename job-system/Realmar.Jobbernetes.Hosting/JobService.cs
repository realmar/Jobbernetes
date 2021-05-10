using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Realmar.Jobbernetes.Framework.Facade;

namespace Realmar.Jobbernetes.Hosting
{
    internal class JobService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IJobbernetes             _jobbernetes;
        private readonly ILogger<JobService>      _logger;

        public JobService(IJobbernetes jobbernetes, IHostApplicationLifetime application, ILogger<JobService> logger)
        {
            _jobbernetes = jobbernetes;
            _application = application;
            _logger      = logger;
        }

        protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _jobbernetes.Run(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    _logger.LogError(e, $"Error occurred while running {nameof(JobService)}");
                }
            }

            _application.StopApplication();
        }
    }
}
