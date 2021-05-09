using System;
using System.Collections.Generic;
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
        private readonly int                         _batchSize;
        private readonly IQueueConsumer<TData>       _consumer;
        private readonly Counter                     _counterProcessed;
        private readonly Counter                     _counterStarted;
        private readonly IJobDispatcher<TData>       _dispatcher;
        private readonly ILogger<Jobbernetes<TData>> _logger;

        public Jobbernetes(IJobDispatcher<TData>       dispatcher,
                           IQueueConsumer<TData>       consumer,
                           ILogger<Jobbernetes<TData>> logger,
                           IOptions<ProcessingOptions> options,
                           IMetricsNameFactory         nameFactory)
        {
            _batchSize  = options.Value.BatchSize;
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
            var jobs = await DispatchJobsAsync(cancellationToken).ConfigureAwait(false);
            await WaitJobsAsync(jobs, cancellationToken).ConfigureAwait(false);
        }

        private async Task<List<(Task, Message<TData>)>> DispatchJobsAsync(CancellationToken cancellationToken)
        {
            var counter = 0;
            var jobs    = new List<(Task, Message<TData>)>();

            await foreach (var message in _consumer.ConsumeAsync(cancellationToken).ConfigureAwait(false))
            {
                counter++;

                try
                {
                    _counterStarted.Inc();

                    var task = _dispatcher.Dispatch(message.Data, cancellationToken);
                    jobs.Add((task, message));
                }
                catch (Exception e)
                {
                    _counterProcessed.WithFail().Inc();
                    _logger.LogError(e, "Failed to start job");
                }

                if (cancellationToken.IsCancellationRequested || counter >= _batchSize)
                {
                    break;
                }
            }

            return jobs;
        }

        private async Task WaitJobsAsync(List<(Task, Message<TData>)> jobs, CancellationToken cancellationToken)
        {
            foreach (var (job, message) in jobs)
            {
                try
                {
                    await job.ConfigureAwait(false);
                    await CommitJob(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception jobException)
                {
                    _logger.LogError(jobException, "Job failed");
                    await RollbackJob(message, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task CommitJob(Message<TData> message, CancellationToken cancellationToken)
        {
            try
            {
                await message.Committer.CommitAsync(cancellationToken).ConfigureAwait(false);
                _counterProcessed.WithSuccess().Inc();
            }
            catch (Exception commitException)
            {
                _counterProcessed.WithFailCommit().Inc();
                _logger.LogError(commitException, "Failed to commit message, this message will replay");
            }
        }

        private async Task RollbackJob(Message<TData> message, CancellationToken cancellationToken)
        {
            try
            {
                await message.Committer.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _counterProcessed.WithFail().Inc();
            }
            catch (Exception rollbackException)
            {
                _logger.LogError(rollbackException, "Failed to rollback job data after job exception");
                _counterProcessed.WithFailRollback().Inc();
            }
        }
    }
}
