using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly AsyncCollection<Action> collection;

        private readonly AsyncCollection<Action> collectionForReentrantItems;

        private readonly Guid nodeId;

        private readonly ConcurrentQueue<Action> concurrentQueueForReentrantItems;

        public DataflowBlock(IActionRunner actionRunner, int maximumNumberOfActionsInQueue, int? maximumDegreeOfParallelism)
        {
            this.actionRunner = actionRunner;
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;

            collection = new AsyncCollection<Action>(new ConcurrentQueue<Action>(), maximumNumberOfActionsInQueue);

            concurrentQueueForReentrantItems = new ConcurrentQueue<Action>();

            collectionForReentrantItems = new AsyncCollection<Action>(concurrentQueueForReentrantItems);

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Action action)
        {
            var task = new DfTask();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

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
                MakeActionRunInCurrentExecutionContextIfAny(runAction);

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

        private Action MakeActionRunInCurrentExecutionContextIfAny(Action action)
        {
            var executionContext = ExecutionContext.Capture();

            return executionContext == null
                ? action
                : (() => ExecutionContext.Run(executionContext, _ => action(), null));
        }

        public DfTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new DfTask<TResult>();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentItem.VisitedNodes.Add(nodeId);

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
                MakeActionRunInCurrentExecutionContextIfAny(runAction);

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

        private async Task<Action> GetAction()
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