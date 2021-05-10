using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Framework.Options;
using Realmar.Jobbernetes.Infrastructure.Metrics;

namespace Realmar.Jobbernetes.Framework.Facade
{
    internal class Jobbernetes<TData> : IJobbernetes
    {
        private readonly int                                    _batchSize;
        private readonly IQueueConsumer<TData>                  _consumer;
        private readonly Counter                                _counterProcessed;
        private readonly Counter                                _counterStarted;
        private readonly IJobDispatcher<TData>                  _dispatcher;
        private readonly ILogger<Jobbernetes<TData>>            _logger;
        private readonly Func<CancellationTokenSource, Watcher> _watcherFactory;

        public Jobbernetes(IJobDispatcher<TData>                  dispatcher,
                           IQueueConsumer<TData>                  consumer,
                           ILogger<Jobbernetes<TData>>            logger,
                           IOptions<ProcessingOptions>            options,
                           Func<CancellationTokenSource, Watcher> watcherFactory,
                           IMetricsNameFactory                    nameFactory)
        {
            _batchSize      = options.Value.BatchSize;
            _dispatcher     = dispatcher;
            _consumer       = consumer;
            _logger         = logger;
            _watcherFactory = watcherFactory;

            var nameStarted = nameFactory.Create("started_total");
            _counterStarted = Metrics.CreateCounter(nameStarted, "Number of started jobs");

            var nameProcessed = nameFactory.Create("processed_total");
            _counterProcessed = Metrics.CreateCounter(nameProcessed, "Number of processed jobs", Labels.Keys.Status);
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            using var tokenSource = new CancellationTokenSource();
            var       watcher     = _watcherFactory.Invoke(tokenSource);

            await _consumer.StartAsync(async (data, token) =>
                            {
                                await watcher.AddJob().ConfigureAwait(false);

                                try
                                {
                                    await ConsumeData(data, token).ConfigureAwait(false);
                                }
                                finally
                                {
                                    watcher.RemoveJob();
                                }
                            }, tokenSource.Token)
                           .ConfigureAwait(false);

            await watcher.WatchAsync(cancellationToken).ConfigureAwait(false);

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
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _counterProcessed.WithFail().Inc();
                _logger.LogError(e, "Job failed to process");
                throw;
            }
        }
    }
}
