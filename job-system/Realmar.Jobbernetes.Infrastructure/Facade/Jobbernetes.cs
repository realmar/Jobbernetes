using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus.Client;
using Realmar.Jobbernetes.Infrastructure.Jobs;
using Realmar.Jobbernetes.Infrastructure.Messaging;
using Realmar.Jobbernetes.Infrastructure.Metrics;
using Realmar.Jobbernetes.Infrastructure.Options.Metrics;

namespace Realmar.Jobbernetes.Infrastructure.Facade
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
            await _consumer.StartAsync(ConsumeData, ProcessReadError, cancellationToken)
                           .ConfigureAwait(false);

            await _consumer.WaitForBatchAsync(cancellationToken).ConfigureAwait(false);

            await _consumer.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task ConsumeData(TData data, CancellationToken token)
        {
            try
            {
                _counterStarted.WithLabels(_options.Value.GetLabelValues()).Inc();
                await _dispatcher.Dispatch(data, token).ConfigureAwait(false);
                _counterProcessed.WithSuccess(_options.Value.GetLabelValues()).Inc();
            }
            catch (Exception e)
            {
                ProcessJobError(e);
                throw;
            }
        }

        private void ProcessReadError(Exception exception, string data)
        {
            ProcessJobError(exception, $" received data = {data}");
        }

        private void ProcessJobError(Exception exception, string? additionalMessage = null)
        {
            if (exception is not OperationCanceledException)
            {
                _counterProcessed.WithFail(_options.Value.GetLabelValues()).Inc();
                var message = "Job failed to process";
                if (additionalMessage != null)
                {
                    message += $" {additionalMessage}";
                }

                _logger.LogError(exception, message);
            }
        }
    }
}
