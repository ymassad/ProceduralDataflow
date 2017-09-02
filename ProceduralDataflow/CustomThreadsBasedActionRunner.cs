using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class TrackingObject
    {
        public HashSet<Guid> VisitedNodes { get; } = new HashSet<Guid>();

        public static ThreadLocal<TrackingObject> CurrentProcessingItem = new ThreadLocal<TrackingObject>(() => null);

    }

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

        public WaitHandle EnqueueAction(Action action)
        {
            ManualResetEvent handle = new ManualResetEvent(false);

            queue.Add(() =>
            {
                action();

                handle.Set();
            });

            return handle;
        }
    }
}