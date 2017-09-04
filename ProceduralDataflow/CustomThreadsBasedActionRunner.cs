using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class CustomThreadsBasedActionRunner : IActionRunner, IStartStopable
    {
        private readonly int numberOfThreads;

        private Thread[] threads;

        private BlockingCollection<Action> queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        public CustomThreadsBasedActionRunner(int numberOfThreads)
        {
            this.numberOfThreads = numberOfThreads;
        }

        public void Start()
        {
            threads = Enumerable.Range(0, numberOfThreads).Select(_ => new Thread(DoIt)).ToArray();

            Array.ForEach(threads, t => t.Start());
        }

        private void DoIt()
        {
            foreach (var action in queue.GetConsumingEnumerable())
            {
                action();
            }
        }

        public void Stop()
        {
            queue.CompleteAdding();
        }

        public Task EnqueueAction(Action action)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            queue.Add(() =>
            {
                action();

                tcs.SetResult(0);
            });

            return tcs.Task;
        }
    }
}