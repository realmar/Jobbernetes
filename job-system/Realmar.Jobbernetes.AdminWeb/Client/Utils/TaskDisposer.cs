using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Realmar.Jobbernetes.AdminWeb.Client.Formatters
{
    internal class TaskDisposer : IAsyncDisposable
    {
        private readonly List<Task> _tasks = new();

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("Disposing Tasks");

            foreach (var task in _tasks)
            {
                try
                {
                    await task.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // This is what we want
                }
            }

            Console.WriteLine("Finished Disposing Tasks");
        }

        public void Add(Task task) => _tasks.Add(task);
    }
}
