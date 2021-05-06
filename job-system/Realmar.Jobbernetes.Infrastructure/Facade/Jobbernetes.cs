using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Facade
{
    internal class Jobbernetes<TData> : IJobbernetes
    {
        private readonly int                         _batchSize;
        private readonly IQueueConsumer<TData>       _consumer;
        private readonly IJobDispatcher<TData>       _dispatcher;
        private readonly List<Task>                  _jobs = new();
        private readonly ILogger<Jobbernetes<TData>> _logger;

        public Jobbernetes(IJobDispatcher<TData>       dispatcher,
                           IQueueConsumer<TData>       consumer,
                           ILogger<Jobbernetes<TData>> logger,
                           IOptions<ProcessingOptions> options)
        {
            _batchSize  = options.Value.BatchSize;
            _dispatcher = dispatcher;
            _consumer   = consumer;
            _logger     = logger;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var counter = 0;

            await foreach (var data in _consumer.Consume(cancellationToken).ConfigureAwait(false))
            {
                counter++;

                try
                {
                    var task = _dispatcher.Dispatch(data, cancellationToken);
                    _jobs.Add(task);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to start job: {e}");
                }

                if (cancellationToken.IsCancellationRequested || counter >= _batchSize)
                {
                    break;
                }
            }

            foreach (var job in _jobs)
            {
                try
                {
                    await job.ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Job failed: {e}");
                }
            }
        }
    }
}
