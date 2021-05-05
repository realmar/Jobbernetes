using System;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.Framework.Jobs
{
    internal class JobDispatcher<TData> : IJobDispatcher<TData>
    {
        private readonly Func<IJob<TData>> _jobFactory;

        public JobDispatcher(Func<IJob<TData>> jobFactory) => _jobFactory = jobFactory;

        public Task Dispatch(TData data, CancellationToken cancellationToken)
        {
            var job = _jobFactory.Invoke();
            return job.Process(data, cancellationToken);
        }
    }
}
