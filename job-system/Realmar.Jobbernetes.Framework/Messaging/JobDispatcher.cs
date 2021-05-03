using System;
using System.Threading;
using System.Threading.Tasks;
using Realmar.Jobbernetes.Framework.Jobs;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class JobDispatcher<TData> : IDataConsumer<TData>
    {
        private readonly Func<IJob<TData>> _jobFactory;

        public JobDispatcher(Func<IJob<TData>> jobFactory) => _jobFactory = jobFactory;

        public Task Consume(TData data, CancellationToken cancellationToken)
        {
            var job = _jobFactory.Invoke();
            return job.Process(data, cancellationToken);
        }
    }
}
