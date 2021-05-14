using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Options.Jobs;
using Realmar.Jobbernetes.Framework.Options.RabbitMQ;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal class EasyNetQBatchConsumer<TData> : EasyNetQBase, IQueueBatchConsumer<TData>, IDisposable
    {
        private readonly IBus                                  _bus;
        private readonly IOptions<JobOptions>                  _jobOptions;
        private readonly ILogger<EasyNetQBatchConsumer<TData>> _logger;
        private readonly CancellationTokenSource               _stopToken = new();
        private          ActionBlock<PullResult<TData>>?       _actionBlock;
        private          Task?                                 _dispatchTask;
        private          CancellationTokenSource?              _manualStopToken;
        private          Func<TData, CancellationToken, Task>? _processor;
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

        public Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken) =>
            StartAsync(processor, null, cancellationToken);

        public async Task StartAsync(Func<TData, CancellationToken, Task> processor,
                                     Action<Exception>?                   readErrorHandler,
                                     CancellationToken                    cancellationToken)
        {
            _readErrorHandler = readErrorHandler;
            _processor        = processor;

            _manualStopToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopToken.Token);

            await PrepareCommunication(_manualStopToken.Token).ConfigureAwait(false);

            _pullingConsumer = _bus.Advanced.CreatePullingConsumer<TData>(Queue, autoAck: false);

            _actionBlock = new(
                ProcessItem,
                new()
                {
                    MaxDegreeOfParallelism = _jobOptions.Value.MaxConcurrentJobs,
                    CancellationToken      = _manualStopToken.Token,
                    MaxMessagesPerTask     = _jobOptions.Value.MaxMessagesPerTask
                });

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods that take one
            _dispatchTask = Task.Run(DispatchMessages);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods that take one
        }

        public async Task WaitForBatchAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_dispatchTask != null)
                {
                    await _dispatchTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // it's ok
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while waiting for dispatcher task to complete");
            }

            try
            {
                if (_actionBlock != null)
                {
                    await _actionBlock.Completion.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // it's ok
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Block threw an error");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopToken?.Cancel();
            return WaitForBatchAsync(cancellationToken);
        }

        private async Task DispatchMessages()
        {
            var counter = 0;

            while (true)
            {
                if (_stopToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Cancellation requested, stop processing new messages");
                    _actionBlock!.Complete();

                    return;
                }

                if (Interlocked.Increment(ref counter) > _jobOptions.Value.BatchSize)
                {
                    _logger.LogInformation("Batch size reached, stop processing new messages");
                    _actionBlock!.Complete();

                    return;
                }

                PullResult<TData>? result = null;
                try
                {
                    result = await _pullingConsumer!.PullAsync(_manualStopToken!.Token).ConfigureAwait(false);

                    if (result.Value.IsAvailable == false)
                    {
                        _logger.LogInformation("No more message available");
                        _actionBlock!.Complete();

                        return;
                    }

                    _actionBlock!.Post(result.Value);
                }
                catch (Exception readException) when (readException is not OperationCanceledException)
                {
                    try
                    {
                        HandleReadException(readException);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception while handling read exception");
                    }

                    if (result != null)
                    {
                        // TODO write to an error queue
                        _logger.LogError("Failed to read message, ACKing it regardless (assuming we"     +
                                         "can't read it next time either). Better solution would be to " +
                                         "write that message to an error queue.");

                        await _pullingConsumer!.AckAsync(result.Value.ReceivedInfo.DeliveryTag).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task ProcessItem(PullResult<TData> result)
        {
            try
            {
                var data = result.Message.Body;

                await _processor!.Invoke(data, _manualStopToken!.Token).ConfigureAwait(false);

                try
                {
                    await _pullingConsumer!.AckAsync(result.ReceivedInfo.DeliveryTag).ConfigureAwait(false);
                    _logger.LogInformation($"Successfully processed job {result.ReceivedInfo}");
                }
                catch (Exception ackException)
                {
                    _logger.LogError(ackException, "Failed to ACK (accept ie. remove from queue) message");
                }
            }
            catch (Exception jobException)
            {
                if (jobException is not OperationCanceledException)
                {
                    _logger.LogError(jobException, $"Error in job {result.ReceivedInfo}");
                }

                try
                {
                    await _pullingConsumer!.RejectAsync(result.ReceivedInfo.DeliveryTag, requeue: true)
                                           .ConfigureAwait(false);
                }
                catch (Exception rejectException)
                {
                    _logger.LogError(rejectException, "Failed to NACK (reject ie. put back into queue) message");
                }
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
