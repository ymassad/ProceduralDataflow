using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class AsyncDataflowBlock : IAsyncDataflowBlock, IStartStopable
    {
        private readonly int? maximumDegreeOfParallelism;

        private readonly BlockingCollection<Func<Task>> collection;

        private readonly BlockingCollection<Func<Task>> collectionForReentrantItems;

        private readonly Guid nodeId;

        public AsyncDataflowBlock(int maximumNumberOfActionsInQueue, int? maximumDegreeOfParallelism)
        {
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;

            collection = new BlockingCollection<Func<Task>>(new ConcurrentQueue<Func<Task>>(), maximumNumberOfActionsInQueue);

            collectionForReentrantItems = new BlockingCollection<Func<Task>>();

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Func<Task> action)
        {
            var task = new DfTask();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

            var executionContext = ExecutionContext.Capture();

            Func<Task> runAction = async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    task.SetException(ex);
                    return;
                }

                TrackingObject.CurrentProcessingItem.Value = currentItem;

                task.SetResult();
            };

            Func<Task> actionToAddToCollection =
                executionContext == null
                    ? runAction
                    : (async () =>
                    {
                        Task actionTask = null;

                        ExecutionContext.Run(executionContext, _ =>
                        {
                            actionTask = runAction();
                        }, null);

                        await actionTask;
                    });

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

        public DfTask<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            var task = new DfTask<TResult>();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

            var executionContext = ExecutionContext.Capture();

            Func<Task> runAction = async() =>
            {
                TResult result;

                try
                {
                    result = await function();
                }
                catch (Exception ex)
                {
                    task.SetException(ex);
                    return;
                }

                TrackingObject.CurrentProcessingItem.Value = currentItem;

                task.SetResult(result);
            };

            Func<Task> actionToAddToCollection =
                executionContext == null
                    ? runAction
                    : (async () =>
                    {
                        Task actionTask = null;

                        ExecutionContext.Run(executionContext, _ =>
                        {
                            actionTask = runAction();
                        }, null);

                        await actionTask;
                    });

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

        private Thread thread;

        public void Start()
        {
            thread = new Thread(() => DoIt().Wait());

            thread.Start();
        }

        private async Task DoIt()
        {
            List<Task> tasks = new List<Task>();

            while (true)
            {
                var action = GetAction();

                if (action == null)
                    return;

                var task = action();

                if (maximumDegreeOfParallelism.HasValue)
                {
                    tasks.Add(task);

                    if (tasks.Count == maximumDegreeOfParallelism)
                    {
                        var taskToRemove = await Task.WhenAny(tasks.ToArray());

                        tasks.Remove(taskToRemove);
                    }
                }
            }
        }

        private Func<Task> GetAction()
        {
            if (collectionForReentrantItems.TryTake(out var reentrantItem))
            {
                return reentrantItem;
            }

            Func<Task> someItem;

            try
            {
                BlockingCollection<Func<Task>>.TakeFromAny(
                    new[] { collectionForReentrantItems, collection },
                    out someItem);
            }
            catch (ArgumentException)
            {
                return null;
            }

            return someItem;
        }

        public void Stop()
        {
            collection.CompleteAdding();
            collectionForReentrantItems.CompleteAdding();
        }
    }
}