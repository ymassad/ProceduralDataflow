using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class DataflowBlock : IDataflowBlock, IStartStopable
    {
        private readonly IActionRunner actionRunner;

        private readonly int? maximumDegreeOfParallelism;

        private readonly AsyncCollection<Func<Task>> collection;

        private readonly AsyncCollection<Func<Task>> collectionForReentrantItems;

        private readonly Guid nodeId;

        private readonly ConcurrentQueue<Func<Task>> concurrentQueueForReentrantItems;

        [ThreadStatic]
        private static Task AddTask;

        public DataflowBlock(IActionRunner actionRunner, int maximumNumberOfActionsInQueue, int? maximumDegreeOfParallelism)
        {
            this.actionRunner = actionRunner;
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;

            collection = new AsyncCollection<Func<Task>>(new ConcurrentQueue<Func<Task>>(), maximumNumberOfActionsInQueue);

            concurrentQueueForReentrantItems = new ConcurrentQueue<Func<Task>>();

            collectionForReentrantItems = new AsyncCollection<Func<Task>>(concurrentQueueForReentrantItems);

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Action action)
        {
            var task = new DfTask();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

            Func<Task> runAction = async () =>
            {
                Exception exception = null;

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                TrackingObject.CurrentProcessingItem.Value = currentItem;

                AddTask = null;

                if (exception == null)
                    task.SetResult();
                else
                    task.SetException(exception);

                if (AddTask != null)
                    await AddTask;
            };

            Func<Task> actionToAddToCollection =
                MakeActionRunInCurrentExecutionContextIfAny(runAction);

            TrackingObject.CurrentProcessingItem.Value = null;

            if (firstVisit)
            {
                AddTask = collection.AddAsync(actionToAddToCollection);
            }
            else
            {
                AddTask = collectionForReentrantItems.AddAsync(actionToAddToCollection);
            }

            return task;
        }

        private Func<Task> MakeActionRunInCurrentExecutionContextIfAny(Func<Task> action)
        {
            var executionContext = ExecutionContext.Capture();

            return executionContext == null
                ? action
                : (async () =>
                {
                    Task task = null;

                    ExecutionContext.Run(executionContext, _ =>
                    {
                        task = action();
                    }, null);

                    await task;
                });
        }

        public DfTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new DfTask<TResult>();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

            Func<Task> runAction = async () =>
            {
                TResult result = default(TResult);

                Exception exception = null;

                try
                {
                    result = function();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                
                TrackingObject.CurrentProcessingItem.Value = currentItem;

                AddTask = null;

                if (exception == null)
                    task.SetResult(result);
                else
                    task.SetException(exception);

                if (AddTask != null)
                    await AddTask;
            };

            Func<Task> actionToAddToCollection =
                MakeActionRunInCurrentExecutionContextIfAny(runAction);

            TrackingObject.CurrentProcessingItem.Value = null;

            if (firstVisit)
            {
                AddTask = collection.AddAsync(actionToAddToCollection);
            }
            else
            {
                AddTask = collectionForReentrantItems.AddAsync(actionToAddToCollection);
            }

            return task;
        }
        
        public void Start()
        {
            DoIt();
        }

        private async Task DoIt()
        {
            List<Task> tasks = new List<Task>();

            while (true)
            {
                var action = await GetAction();

                if (action == null)
                    return;

                var task = actionRunner.EnqueueAction(action);

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

        private async Task<Func<Task>> GetAction()
        {
            if (!concurrentQueueForReentrantItems.IsEmpty)
            {
                var reentrantItemResult = await collectionForReentrantItems.TryTakeAsync();

                if (reentrantItemResult.Success)
                {
                    return reentrantItemResult.Item;
                }
            }

            var itemResult = await new[] { collectionForReentrantItems, collection }.TryTakeFromAnyAsync();

            if (!itemResult.Success)
                return null;

            return itemResult.Item;
        }

        public void Stop()
        {
            collection.CompleteAdding();
            collectionForReentrantItems.CompleteAdding();
        }
    }
}