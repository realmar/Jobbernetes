using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    public class EasyNetQBatchConsumer<TData> : EasyNetQBase, IQueueBatchConsumer<TData>, IDisposable
    {
        private readonly TaskCompletionSource                  _batchCompletionSource = new();
        private readonly IBus                                  _bus;
        private readonly IOptions<JobOptions>                  _jobOptions;
        private readonly ConcurrentBag<Job>                    _jobs = new();
        private readonly ILogger<EasyNetQBatchConsumer<TData>> _logger;
        private readonly CancellationTokenSource               _stopToken = new();

        public EasyNetQBatchConsumer(IBus                                  bus,
                                     IOptions<RabbitMQConsumerOptions>     options,
                                     IOptions<JobOptions>                  jobOptions,
                                     ILogger<EasyNetQBatchConsumer<TData>> logger) : base(options, bus)
        {
            _bus        = bus;
            _jobOptions = jobOptions;
            _logger     = logger;
        }

        public void Dispose()
        {
            _stopToken.Dispose();
        }

        public Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopToken.Token);
            return Task.Run(async () =>
            {
                await PrepareCommunication(cancellationToken).ConfigureAwait(false);

                var batchSize       = _jobOptions.Value.BatchSize;
                var pullingConsumer = _bus.Advanced.CreatePullingConsumer<TData>(Queue, autoAck: false);

                for (var counter = 0; counter < batchSize; counter++)
                {
                    if (_stopToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Cancellation requested");
                        break;
                    }

                    var result = await pullingConsumer.PullAsync(cancellationToken).ConfigureAwait(false);
                    if (result.IsAvailable == false)
                    {
                        _logger.LogInformation("No more message available");
                        break;
                    }

                    var data = result.Message.Body;
                    var job  = processor.Invoke(data, linkedToken.Token);

                    _jobs.Add(new()
                    {
                        Task     = job,
                        Result   = result,
                        Consumer = pullingConsumer
                    });
                }

                _batchCompletionSource.TrySetResult();
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopToken.Cancel();
        }

        public async Task WaitForBatchAsync(CancellationToken cancellationToken)
        {
            await _batchCompletionSource.Task.ConfigureAwait(false);

            while (_jobs.IsEmpty == false)
            {
                if (_jobs.TryTake(out var job))
                {
                    var (task, result, consumer) = job;
                    try
                    {
                        await task.ConfigureAwait(false);
                        await consumer.AckAsync(result.ReceivedInfo.DeliveryTag).ConfigureAwait(false);
                        _logger.LogInformation($"Successfully processed job {result.ReceivedInfo}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error in job {result.ReceivedInfo}");
                        await consumer.RejectAsync(result.ReceivedInfo.DeliveryTag, requeue: true).ConfigureAwait(false);
                    }
                }
            }
        }

        private struct Job
        {
            public Task                                Task;
            public PullResult<TData>                   Result;
            public IPullingConsumer<PullResult<TData>> Consumer;

            public void Deconstruct(out Task                                task,
                                    out PullResult<TData>                   result,
                                    out IPullingConsumer<PullResult<TData>> consumer) =>
                (task, result, consumer) = (Task, Result, Consumer);
        }
    }
}
