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

        private readonly BlockingCollection<Action> collection;

        private readonly BlockingCollection<Action> collectionForReentrantItems;

        private readonly Guid nodeId;


        public CustomThreadsBasedActionRunner(int numberOfThreads, int maximumNumberOfActionsInQueue)
        {
            this.numberOfThreads = numberOfThreads;
            collection = new BlockingCollection<Action>(new ConcurrentQueue<Action>(), maximumNumberOfActionsInQueue);

            collectionForReentrantItems = new BlockingCollection<Action>();

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Action action)
        {
            var task = new DfTask();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if(firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

            var executionContext = ExecutionContext.Capture();

            Action runAction = () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    task.SetException(ex);
                    return;
                }

                TrackingObject.CurrentProcessingItem.Value = currentItem;

                task.SetResult();
            };

            Action actionToAddToCollection =
                executionContext == null
                    ? runAction
                    : (() => ExecutionContext.Run(executionContext, _ => runAction(), null));

            TrackingObject.CurrentProcessingItem.Value = null;

            if (firstVisit)
            {
                collection.Add(actionToAddToCollection);
            }
            else
            {
                collectionForReentrantItems.Add(actionToAddToCollection);
            }

            return task;
        }

        public DfTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new DfTask<TResult>();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

            var executionContext = ExecutionContext.Capture();

            Action runAction = () =>
            {
                TResult result;

                try
                {
                    result = function();
                }
                catch (Exception ex)
                {
                    task.SetException(ex);
                    return;
                }

                TrackingObject.CurrentProcessingItem.Value = currentItem;

                task.SetResult(result);
            };

            Action actionToAddToCollection =
                executionContext == null
                    ? runAction
                    : (() => ExecutionContext.Run(executionContext, _ => runAction(), null));


            TrackingObject.CurrentProcessingItem.Value = null;

            if (firstVisit)
            {
                collection.Add(actionToAddToCollection);
            }
            else
            {
                collectionForReentrantItems.Add(actionToAddToCollection);
            }

            return task;
        }

        public void Start()
        {
            threads = Enumerable.Range(0, numberOfThreads).Select(_ => new Thread(DoIt)).ToArray();

            Array.ForEach(threads, t => t.Start());
        }

        private void DoIt()
        {
            while (true)
            {
                while (collectionForReentrantItems.TryTake(out var reentrantItem))
                {
                    reentrantItem();
                }

                Action someItem;

                try
                {
                    BlockingCollection<Action>.TakeFromAny(
                        new[] {collectionForReentrantItems, collection},
                        out someItem);
                }
                catch (ArgumentException)
                {
                    return;
                }

                someItem();
            }

        }

        public void Stop()
        {
            collection.CompleteAdding();
            collectionForReentrantItems.CompleteAdding();

            //Array.ForEach(threads, t => t.Join());
        }
    }
}