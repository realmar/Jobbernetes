using System;
using System.Threading;
using System.Threading.Tasks;
using Realmar.Jobbernetes.Framework.Jobs;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class NullQueueConsumer : IQueueConsumer<NullInput>
    {
        public Task StartAsync(Func<NullInput, CancellationToken, Task> processor, CancellationToken cancellationToken) =>
            Task.Run(async () =>
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    await processor.Invoke(NullInput.Default, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
