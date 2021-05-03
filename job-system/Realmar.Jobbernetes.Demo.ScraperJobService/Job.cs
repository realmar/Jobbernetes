using System;
using System.Threading;
using System.Threading.Tasks;
using Realmar.Jobbernetes.Demo.GRPC;
using Realmar.Jobbernetes.Framework.Jobs;

namespace Realmar.Jobbernetes.Demo.ScraperJobService
{
    public class Job : IJob<ImageIngress>
    {
        public Task Process(ImageIngress data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
