using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus.Client;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Framework.Options.Metrics;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Facade
{
    internal class Jobbernetes<TData> : IJobbernetes
    {
        private readonly IQueueBatchConsumer<TData>    _consumer;
        private readonly IMetricFamily<ICounter>       _counterProcessed;
        private readonly IMetricFamily<ICounter>       _counterStarted;
        private readonly IJobDispatcher<TData>         _dispatcher;
        private readonly ILogger<Jobbernetes<TData>>   _logger;
        private readonly IOptions<MetricPusherOptions> _options;

        public Jobbernetes(IJobDispatcher<TData>         dispatcher,
                           IQueueBatchConsumer<TData>    consumer,
                           ILogger<Jobbernetes<TData>>   logger,
                           IMetricsNameFactory           nameFactory,
                           IMetricFactory                metricFactory,
                           IOptions<MetricPusherOptions> options)
        {
            _dispatcher = dispatcher;
            _consumer   = consumer;
            _logger     = logger;
            _options    = options;

            var nameStarted = nameFactory.Create("started_total");
            _counterStarted = metricFactory.CreateJobCounter(nameStarted, "Number of started jobs");

            var nameProcessed = nameFactory.Create("processed_total");
            _counterProcessed = metricFactory.CreateJobCounter(nameProcessed, "Number of processed jobs", Labels.Keys.Status);
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
                _counterProcessed.WithSuccess(_options.Value.GetLabelValues()).Inc();
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
                _counterProcessed.WithFail(_options.Value.GetLabelValues()).Inc();
                _logger.LogError(exception, "Job failed to process");
            }
        }
    }
}
