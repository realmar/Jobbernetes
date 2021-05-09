using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Realmar.Jobbernetes.Framework.Jobs;

namespace Realmar.Jobbernetes.Framework.Messaging
{
    public class NullQueueConsumer : IQueueConsumer<NullInput>
    {
        public IAsyncEnumerable<Message<NullInput>> ConsumeAsync(CancellationToken cancellationToken) =>
            ConsumeSync(cancellationToken).ToAsyncEnumerable();

        private static IEnumerable<Message<NullInput>> ConsumeSync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                yield return new(NullInput.Default, NullCommitter.Default);
            }
        }

        private class NullCommitter : IMessageCommitter
        {
            public static NullCommitter Default { get; } = new();

            public Task CommitAsync(CancellationToken   cancellationToken) => Task.CompletedTask;
            public Task RollbackAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}
