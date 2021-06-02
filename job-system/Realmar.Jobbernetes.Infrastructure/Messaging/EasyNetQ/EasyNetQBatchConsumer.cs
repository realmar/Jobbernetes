using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Realmar.Jobbernetes.Framework.Messaging.Serialization;
using Realmar.Jobbernetes.Framework.Options.Jobs;
using Realmar.Jobbernetes.Framework.Options.RabbitMQ;
using ISerializer = Realmar.Jobbernetes.Framework.Messaging.Serialization.ISerializer;

namespace Realmar.Jobbernetes.Framework.Messaging.EasyNetQ
{
    internal class EasyNetQBatchConsumer<TData> : IQueueBatchConsumer<TData>, IDisposable
    {
        private readonly IBus                                  _bus;
        private readonly IOptions<JobOptions>                  _jobOptions;
        private readonly ILogger<EasyNetQBatchConsumer<TData>> _logger;
        private readonly IOptions<RabbitMQConsumerOptions>     _rabbitMqOptions;
        private readonly ISerializer                           _serializer;
        private readonly CancellationTokenSource               _stopToken = new();
        private          ActionBlock<PullResult<TData>>?       _actionBlock;
        private          Task?                                 _dispatchTask;
        private          CancellationTokenSource?              _manualStopToken;
        private          Func<TData, CancellationToken, Task>? _processor;
        private          IPullingConsumer<PullResult>?         _pullingConsumer;
        private          Action<Exception, string>?            _readErrorHandler;

        public EasyNetQBatchConsumer(IBus                                  bus,
                                     ISerializer                           serializer,
                                     IOptions<RabbitMQConsumerOptions>     rabbitMqOptions,
                                     IOptions<JobOptions>                  jobOptions,
                                     ILogger<EasyNetQBatchConsumer<TData>> logger)
        {
            _bus             = bus;
            _serializer      = serializer;
            _rabbitMqOptions = rabbitMqOptions;
            _jobOptions      = jobOptions;
            _logger          = logger;
        }

        public void Dispose()
        {
            _stopToken.Dispose();
        }

        public Task StartAsync(Func<TData, CancellationToken, Task> processor, CancellationToken cancellationToken) =>
            StartAsync(processor, null, cancellationToken);

        public async Task StartAsync(Func<TData, CancellationToken, Task> processor,
                                     Action<Exception, string>?           readErrorHandler,
                                     CancellationToken                    cancellationToken)
        {
            _readErrorHandler = readErrorHandler;
            _processor        = processor;

            _manualStopToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopToken.Token);

            var (_, queue) = await _bus.DeclareAndBindQueueAsync(_rabbitMqOptions, cancellationToken)
                                       .ConfigureAwait(false);

            _pullingConsumer = _bus.Advanced.CreatePullingConsumer(queue, autoAck: false);

            _actionBlock = new(
                ProcessItem,
                new()
                {
                    MaxDegreeOfParallelism = _jobOptions.Value.MaxDegreeOfParallelism,
                    CancellationToken      = _manualStopToken.Token
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

                PullResult? result = null;
                try
                {
                    result = await _pullingConsumer!.PullAsync(_manualStopToken!.Token).ConfigureAwait(false);

                    if (result.Value.IsAvailable == false)
                    {
                        _logger.LogInformation("No more message available");
                        _actionBlock!.Complete();

                        return;
                    }

                    var data = _serializer.Deserialize<TData>(result.Value.Body);

                    _actionBlock!.Post(PullResult<TData>.Available(
                                           messagesCount: result.Value.MessagesCount,
                                           receivedInfo: result.Value.ReceivedInfo,
                                           message: new Message<TData>(
                                               body: data,
                                               properties: result.Value.Properties)));
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

                    // TODO write to an error queue
                    _logger.LogError("Failed to read message, ACKing it regardless (assuming we"     +
                                     "can't read it next time either). Better solution would be to " +
                                     "write that message to an error queue.");

                    await _pullingConsumer!.AckAsync(result!.Value.ReceivedInfo.DeliveryTag).ConfigureAwait(false);
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
                if (_logger.IsEnabled(LogLevel.Debug) && jobException is not OperationCanceledException)
                {
                    _logger.LogDebug(jobException, $"Error in job {result.ReceivedInfo}");
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
            var dataString = "<no data provided>";
            if (readException.Data.Contains(Constants.StringDataKey) &&
                readException.Data[Constants.StringDataKey] is string str)
            {
                dataString = str;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(readException, $"Failed to read message data string = {dataString}");
            }

            try
            {
                _readErrorHandler?.Invoke(readException, dataString);
            }
            catch (Exception handlerException)
            {
                _logger.LogError(handlerException, "Error in read error handler");
            }
        }
    }
}
