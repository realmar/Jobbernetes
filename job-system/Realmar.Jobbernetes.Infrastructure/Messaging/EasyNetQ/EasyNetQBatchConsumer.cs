using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    public class EasyNetQBatchConsumer<TData> : EasyNetQBase, IQueueBatchConsumer<TData>, IDisposable
    {
        private readonly IBus                                  _bus;
        private readonly IOptions<JobOptions>                  _jobOptions;
        private readonly ILogger<EasyNetQBatchConsumer<TData>> _logger;
        private readonly CancellationTokenSource               _queueEmptyToken = new();
        private readonly CancellationTokenSource               _stopToken       = new();
        private          ActionBlock<ulong>?                   _block;
        private          CancellationTokenSource?              _linkedToken;
        private          Func<TData, CancellationToken, Task>  _processor;
        private          IPullingConsumer<PullResult<TData>>?  _pullingConsumer;
        private          Action<Exception>?                    _readErrorHandler;

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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopToken?.Cancel();
            return Task.CompletedTask;
        }

        public async Task WaitForBatchAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _block.Completion.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // that is ok
            }
        }

        public Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken) =>
            StartAsync(processor, null, cancellationToken);

        public async Task StartAsync(Func<TData, CancellationToken, Task> processor,
                                     Action<Exception>?                   readErrorHandler,
                                     CancellationToken                    cancellationToken)
        {
            _readErrorHandler = readErrorHandler;
            _processor        = processor;

            await PrepareCommunication(cancellationToken).ConfigureAwait(false);

            _linkedToken     = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopToken.Token);
            _pullingConsumer = _bus.Advanced.CreatePullingConsumer<TData>(Queue, autoAck: false);

            _block = new(_ => ProcessItem(),
                         new()
                         {
                             MaxDegreeOfParallelism = _jobOptions.Value.MaxConcurrentJobs,
                             CancellationToken      = _linkedToken.Token,
                             MaxMessagesPerTask     = _jobOptions.Value.MaxConcurrentJobs
                         });

            var stats        = await _bus.Advanced.GetQueueStatsAsync(Queue, cancellationToken).ConfigureAwait(false);
            var messageCount = stats.MessagesCount;

            var batchSize = Math.Clamp((ulong) _jobOptions.Value.BatchSize, 0ul, messageCount);
            for (var counter = 0ul; counter < batchSize; counter++)
            {
                _block.Post(counter);
            }

            _block.Complete();
        }

        private async Task ProcessItem()
        {
            if (_queueEmptyToken.IsCancellationRequested)
            {
                return;
            }

            if (_linkedToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancellation requested");
                return;
            }

            try
            {
                var result = await _pullingConsumer.PullAsync(_linkedToken.Token).ConfigureAwait(false);

                if (result.IsAvailable == false)
                {
                    _logger.LogInformation("No more message available");
                    _queueEmptyToken.Cancel();

                    return;
                }

                await ProcessJob(_processor, result).ConfigureAwait(false);
            }
            catch (Exception readException) when (readException is not OperationCanceledException)
            {
                HandleReadException(readException);
            }
        }

        private async Task ProcessJob(Func<TData, CancellationToken, Task> processor, PullResult<TData> result)
        {
            try
            {
                var data = result.Message.Body;

                await processor.Invoke(data, _linkedToken.Token).ConfigureAwait(false);
                await _pullingConsumer.AckAsync(result.ReceivedInfo.DeliveryTag).ConfigureAwait(false);
                _logger.LogInformation($"Successfully processed job {result.ReceivedInfo}");
            }
            catch (Exception jobException)
            {
                _logger.LogError(jobException, $"Error in job {result.ReceivedInfo}");
                await _pullingConsumer.RejectAsync(result.ReceivedInfo.DeliveryTag, requeue: true)
                                      .ConfigureAwait(false);
            }
        }

        private void HandleReadException(Exception readException)
        {
            _logger.LogError(readException, "Failed to read message");

            try
            {
                _readErrorHandler?.Invoke(readException);
            }
            catch (Exception handlerException)
            {
                _logger.LogError(handlerException, "Error in read error handler");
            }
        }
    }
}
