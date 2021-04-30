using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Realmar.Jobbernetes.Framework;
using Realmar.Jobbernetes.Hosting;

namespace Realmar.Jobbernetes.DemoJob
{
    public class DemoJobService : JobService
    {
        public DemoJobService(IJobbernetes jobbernetes, IHostApplicationLifetime application) : base(jobbernetes, application) { }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
