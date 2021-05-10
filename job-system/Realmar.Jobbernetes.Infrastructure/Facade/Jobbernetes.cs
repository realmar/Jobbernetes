using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Prometheus;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Facade
{
    internal class Jobbernetes<TData> : IJobbernetes
    {
        private readonly IQueueBatchConsumer<TData>  _consumer;
        private readonly Counter                     _counterProcessed;
        private readonly Counter                     _counterStarted;
        private readonly IJobDispatcher<TData>       _dispatcher;
        private readonly ILogger<Jobbernetes<TData>> _logger;

        public Jobbernetes(IJobDispatcher<TData>       dispatcher,
                           IQueueBatchConsumer<TData>  consumer,
                           ILogger<Jobbernetes<TData>> logger,
                           IMetricsNameFactory         nameFactory)
        {
            _dispatcher = dispatcher;
            _consumer   = consumer;
            _logger     = logger;

            var nameStarted = nameFactory.Create("started_total");
            _counterStarted = Metrics.CreateCounter(nameStarted, "Number of started jobs");

            var nameProcessed = nameFactory.Create("processed_total");
            _counterProcessed = Metrics.CreateCounter(nameProcessed, "Number of processed jobs", Labels.Keys.Status);
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await _consumer.StartAsync(ConsumeData, ProcessJobError, cancellationToken)
                           .ConfigureAwait(false);

            await _consumer.WaitForBatchAsync(cancellationToken).ConfigureAwait(false);

            await _consumer.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task ConsumeData(TData data, CancellationToken token)
        {
            try
            {
                _counterStarted.Inc();
                await _dispatcher.Dispatch(data, token).ConfigureAwait(false);
                _counterProcessed.WithSuccess().Inc();
            }
            catch (Exception e)
            {
                ProcessJobError(e);
                throw;
            }
        }

        private void ProcessJobError(Exception exception)
        {
            if (exception is not OperationCanceledException)
            {
                _counterProcessed.WithFail().Inc();
                _logger.LogError(exception, "Job failed to process");
            }
        }
    }
}
