using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNext;
using Microsoft.Extensions.Logging;
using Realmar.Jobbernetes.AdminWeb.Shared.Models;

namespace Realmar.Jobbernetes.AdminWeb.Server.Services
{
    public class JobService : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly List<JobInstance>       _jobs;
        private readonly ILogger<JobService>     _logger;
        private readonly Random                  _random = new();
        private readonly Task                    _ticker;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;
            _jobs = new()
            {
                new()
                {
                    Object = new()
                    {
                        Name     = "Spaceship Job",
                        TopError = "Connection Timeout"
                    },
                    SuccessProbability = 0.12,
                    Increase           = new(10, 20)
                },
                new()
                {
                    Object = new()
                    {
                        Name     = "Airplane Job",
                        TopError = "Server Returned 500"
                    },
                    SuccessProbability = 0.62,
                    Increase           = new(80, 120)
                },
                new()
                {
                    Object = new()
                    {
                        Name     = "Car Job",
                        TopError = "Resource Unavailable"
                    },
                    SuccessProbability = 0.92,
                    Increase           = new(200, 500)
                },
                new()
                {
                    Object = new()
                    {
                        Name     = "Jet Job",
                        TopError = "Pod Error"
                    },
                    SuccessProbability = 0.82,
                    Increase           = new(120, 180)
                }
            };

            _ticker = Task.Run(Tick);
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await _ticker.ConfigureAwait(false);
        }

        private async Task Tick()
        {
            while (_cts.IsCancellationRequested == false)
            {
                try
                {
                    lock (_jobs)
                    {
                        foreach (var job in _jobs)
                        {
                            ProcessingMetrics         metrics;
                            Action<ProcessingMetrics> setter;

                            if (_random.NextBoolean(job.SuccessProbability))
                            {
                                metrics = job.Object.Success;
                                setter  = x => job.Object.Success = x;
                            }
                            else
                            {
                                metrics = job.Object.Failed;
                                setter  = x => job.Object.Failed = x;
                            }

                            var count      = metrics.Count + _random.Next(job.Increase.Start.Value, job.Increase.End.Value);
                            var throughput = (int) Math.Round(count / (DateTime.UtcNow - job.StartTime).TotalSeconds);

                            setter.Invoke(new(count, throughput));
                        }
                    }
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _logger.LogError(e, e.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
            }
        }

        public void AddJob(AddJobModel model)
        {
            lock (_jobs)
            {
                _jobs.Add(new()
                {
                    Object = new()
                    {
                        Name     = model.Name,
                        TopError = model.TopError
                    },
                    SuccessProbability = model.SuccessProbability / 100d,
                    Increase           = new(model.IncreaseMin, model.IncreaseMax)
                });
            }
        }

        public void DeleteJob(Guid guid)
        {
            lock (_jobs)
            {
                _jobs.RemoveAll(instance => instance.Object.Guid.Equals(guid));
            }
        }

        public OverviewSummary GetSummary()
        {
            ProcessingMetrics Sum(Func<JobInstance, ProcessingMetrics> selector) =>
                _jobs.Select(selector)
                     .Aggregate(new ProcessingMetrics(), (a, b) => a + b);

            lock (_jobs)
            {
                var success     = Sum(instance => instance.Object.Success);
                var failed      = Sum(instance => instance.Object.Failed);
                var total       = success + failed;
                var successRate = (double) success.Count / (failed.Count + success.Count);

                // ReSharper disable once ComplexConditionExpression
                var topErrors = _jobs
                               .Select(instance => instance.Object)
                               .OrderByDescending(job => job.Failed.Count)
                               .Select(job => new Error((double) job.Failed.Count / failed.Count * 100d, job.TopError))
                               .Take(3)
                                // ReSharper disable once TooManyChainedReferences
                               .ToArray();

                return new(
                    successRate * 100d,
                    success,
                    failed,
                    total,
                    topErrors);
            }
        }

        public List<Job> GetJobs()
        {
            lock (_jobs)
            {
                return _jobs.Select(instance => instance.Object.Clone()).ToList();
            }
        }

        private class JobInstance
        {
            public readonly DateTime StartTime = DateTime.UtcNow;
            public          Range    Increase;
            public          Job      Object;
            public          double   SuccessProbability;
        }
    }
}
