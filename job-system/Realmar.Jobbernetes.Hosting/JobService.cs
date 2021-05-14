using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus.Client.MetricPusher;
using Realmar.Jobbernetes.Framework.Facade;

namespace Realmar.Jobbernetes.Hosting
{
    internal class JobService : BackgroundService
    {
        private readonly IHostApplicationLifetime _application;
        private readonly IJobbernetes             _jobbernetes;
        private readonly ILogger<JobService>      _logger;
        private readonly IMetricPusher            _metricPusher;

        public JobService(IJobbernetes             jobbernetes,
                          IMetricPusher            metricPusher,
                          IHostApplicationLifetime application,
                          ILogger<JobService>      logger)
        {
            _jobbernetes  = jobbernetes;
            _metricPusher = metricPusher;
            _application  = application;
            _logger       = logger;
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
            finally
            {
                await _metricPusher.PushAsync().ConfigureAwait(false);
            }

            _application.StopApplication();
        }
    }
}
