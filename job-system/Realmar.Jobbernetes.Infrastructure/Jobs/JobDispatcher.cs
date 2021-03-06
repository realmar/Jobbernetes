using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Infrastructure.Jobs
{
    internal class JobDispatcher<TData> : IJobDispatcher<TData>
    {
        private readonly Func<IJob<TData>> _jobFactory;

        public JobDispatcher(Func<IJob<TData>> jobFactory) => _jobFactory = jobFactory;

        public Task Dispatch(TData data, CancellationToken cancellationToken = default)
        {
            var job = _jobFactory.Invoke();
            return job.ProcessAsync(data, cancellationToken);
        }
    }
}
