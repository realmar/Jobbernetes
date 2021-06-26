using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Realmar.Jobbernetes.AdminWeb.Server.Services;
using Realmar.Jobbernetes.AdminWeb.Shared.Models;
using Realmar.Jobbernetes.AdminWeb.Shared.Primitives;

namespace Realmar.Jobbernetes.AdminWeb.Server.Hubs
{
    internal class OverviewHub : Hub
    {
        private readonly JobService _jobService;

        public OverviewHub(JobService jobService) => _jobService = jobService;

        [PublicAPI]
        public async IAsyncEnumerable<OverviewSummary> StreamSummary(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return _jobService.GetSummary();
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }


        [PublicAPI]
        public async IAsyncEnumerable<List<Job>> StreamJobs(
            HealthState                                state,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var jobs = _jobService.GetJobs();

                yield return state switch
                {
                    HealthState.Critical => jobs.Where(job => job.SuccessRate <= 0.2).ToList(),
                    HealthState.Warning  => jobs.Where(job => job.SuccessRate is > 0.2 and < 0.8).ToList(),
                    HealthState.Good     => jobs.Where(job => job.SuccessRate >= 0.8).ToList(),
                    _                    => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                };

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
