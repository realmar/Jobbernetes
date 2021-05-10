using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Facade
{
    /// Extreme tight coupling with
    /// <see cref="Jobbernetes{TData}" />
    internal class Watcher
    {
        private readonly TaskCompletionSource _batchFinished = new();

        private readonly object               _dateTimeLock = new();
        private readonly TaskCompletionSource _jobsFinished = new();

        private readonly ILogger<Watcher>        _logger;
        private readonly CancellationTokenSource _queueTokenSource;
        private volatile int                     _activeJobsCounter;
        private volatile int                     _jobCounter;
        private          DateTime                _lastMessage = DateTime.UtcNow;


        public Watcher(CancellationTokenSource queueTokenSource, ILogger<Watcher> logger, IOptions<ProcessingOptions> options)
        {
            _queueTokenSource = queueTokenSource;
            _logger           = logger;
            _jobCounter       = options.Value.BatchSize;
        }

        /// <exception cref="T:System.Threading.Tasks.TaskCanceledException">When batch size is reached.</exception>
        public async Task AddJob()
        {
            IBus bus;

            UpdateDateTime();
            if (Interlocked.Decrement(ref _jobCounter) < 0)
            {
                _batchFinished.TrySetResult();
                _logger.LogInformation("Batch size reached, stop processing messages ...");

                if (_activeJobsCounter <= 0)
                {
                    _queueTokenSource.Cancel();
                }

                await _jobsFinished.Task.ConfigureAwait(false);

                throw new TaskCanceledException();
            }

            Interlocked.Increment(ref _activeJobsCounter);
        }

        public void RemoveJob()
        {
            if (Interlocked.Decrement(ref _activeJobsCounter) < 0 && _batchFinished.Task.IsCompleted)
            {
                _queueTokenSource.Cancel();
                _jobsFinished.TrySetResult();
            }
        }

        public async Task WatchAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (_queueTokenSource.IsCancellationRequested)
                {
                    break;
                }

                TimeSpan delta;
                lock (_dateTimeLock)
                {
                    delta = DateTime.UtcNow - _lastMessage;
                }

                var timeout = delta.TotalMilliseconds > 8d * 1000d;

                if (timeout || cancellationToken.IsCancellationRequested || _batchFinished.Task.IsCompleted)
                {
                    if (timeout)
                    {
                        _logger.LogInformation("No more messages found in queue, signaling to parent ...");
                    }

                    await WaitJobsAndCancelQueueAsync().ConfigureAwait(false);
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(0.2d)).ConfigureAwait(false);
            }
        }

        private void UpdateDateTime()
        {
            lock (_dateTimeLock)
            {
                _lastMessage = DateTime.UtcNow;
            }
        }

        private async Task WaitJobsAndCancelQueueAsync()
        {
            if (_activeJobsCounter > 0)
            {
                await _jobsFinished.Task.ConfigureAwait(false);
            }

            _queueTokenSource.Cancel();
        }
    }
}
