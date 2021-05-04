using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Realmar.Jobbernetes.Framework.Jobs;
using Realmar.Jobbernetes.Framework.Messaging;

namespace Realmar.Jobbernetes.Framework.Facade
{
    public class Jobbernetes<TData> : IJobbernetes
    {
        private readonly IJobDispatcher<TData>       _dispatcher;
        private readonly List<Task>                  _jobs = new();
        private readonly ILogger<Jobbernetes<TData>> _logger;
        private readonly IQueueConsumer<TData>       _producer;

        public Jobbernetes(IJobDispatcher<TData>       dispatcher,
                           IQueueConsumer<TData>       producer,
                           ILogger<Jobbernetes<TData>> logger)
        {
            _dispatcher = dispatcher;
            _producer   = producer;
            _logger     = logger;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await foreach (var data in _producer.Consume(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    var task = _dispatcher.Dispatch(data, cancellationToken);
                    _jobs.Add(task);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to start job: {e}");
                }

                if (cancellationToken.IsCancellationRequested)
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
