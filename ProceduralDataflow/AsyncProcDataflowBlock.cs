using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class AsyncProcDataflowBlock : IAsyncProcDataflowBlock, IStartStopable
    {
        private readonly int? maximumDegreeOfParallelism;

        private readonly AsyncCollection<Func<Task>> collection;

        private readonly AsyncCollection<Func<Task>> collectionForReentrantItems;

        private readonly Guid nodeId;

        private ConcurrentQueue<Func<Task>> concurrentQueueForReentrantItems;

        public AsyncProcDataflowBlock(int maximumNumberOfActionsInQueue, int? maximumDegreeOfParallelism)
        {
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;

            collection = new AsyncCollection<Func<Task>>(new ConcurrentQueue<Func<Task>>(), maximumNumberOfActionsInQueue);

            concurrentQueueForReentrantItems = new ConcurrentQueue<Func<Task>>();

            collectionForReentrantItems = new AsyncCollection<Func<Task>>(concurrentQueueForReentrantItems);

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Func<Task> action)
        {
            var task = new DfTask();

            var currentListOfVisitedNodes = ListOfVisitedNodes.Current.Value ?? (ListOfVisitedNodes.Current.Value = new ListOfVisitedNodes());

            var firstVisit = !currentListOfVisitedNodes.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentListOfVisitedNodes.VisitedNodes.Add(nodeId);

            Func<Task> runAction = async () =>
            {
                Exception exception = null;

                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                ListOfVisitedNodes.Current.Value = currentListOfVisitedNodes;

                DfTask.AsyncBlockingTask = null;

                if (exception == null)
                    task.SetResult();
                else
                    task.SetException(exception);

                if (DfTask.AsyncBlockingTask != null)
                    await DfTask.AsyncBlockingTask;
            };

            Func<Task> actionToAddToCollection =
                MakeActionRunInCurrentExecutionContextIfAny(runAction);

            ListOfVisitedNodes.Current.Value = null;

            if (firstVisit)
            {
                DfTask.AsyncBlockingTask = collection.AddAsync(actionToAddToCollection);
            }
            else
            {
                DfTask.AsyncBlockingTask = collectionForReentrantItems.AddAsync(actionToAddToCollection);
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

        public DfTask<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            var task = new DfTask<TResult>();

            var currentListOfVisitedNodes = ListOfVisitedNodes.Current.Value ?? (ListOfVisitedNodes.Current.Value = new ListOfVisitedNodes());

            var firstVisit = !currentListOfVisitedNodes.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentListOfVisitedNodes.VisitedNodes.Add(nodeId);

            Func<Task> runAction = async() =>
            {
                TResult result = default(TResult);

                Exception exception = null;

                try
                {
                    result = await function();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                ListOfVisitedNodes.Current.Value = currentListOfVisitedNodes;

                DfTask.AsyncBlockingTask = null;

                if (exception == null)
                    task.SetResult(result);
                else
                    task.SetException(exception);

                if (DfTask.AsyncBlockingTask != null)
                    await DfTask.AsyncBlockingTask;
            };

            Func<Task> actionToAddToCollection =
                MakeActionRunInCurrentExecutionContextIfAny(runAction);

            ListOfVisitedNodes.Current.Value = null;

            if (firstVisit)
            {
                DfTask.AsyncBlockingTask = collection.AddAsync(actionToAddToCollection);
            }
            else
            {
                DfTask.AsyncBlockingTask = collectionForReentrantItems.AddAsync(actionToAddToCollection);
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

            if(!itemResult.Success)
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