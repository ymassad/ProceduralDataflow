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
    public class ProcDataflowBlock : IProcDataflowBlock, IStartStopable
    {
        private readonly IActionRunner actionRunner;

        private readonly int? maximumDegreeOfParallelism;

        private readonly AsyncCollection<Func<Task>> collection;

        private readonly AsyncCollection<Func<Task>> collectionForReentrantItems;

        private readonly Guid nodeId;

        public ProcDataflowBlock(IActionRunner actionRunner, int maximumNumberOfActionsInQueue, int? maximumDegreeOfParallelism)
        {
            this.actionRunner = actionRunner;
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;

            collection = new AsyncCollection<Func<Task>>(new ConcurrentQueue<Func<Task>>(), maximumNumberOfActionsInQueue);

            collectionForReentrantItems = new AsyncCollection<Func<Task>>(new ConcurrentQueue<Func<Task>>());

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Action action)
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
                    action();
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

        public DfTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new DfTask<TResult>();

            var currentListOfVisitedNodes = ListOfVisitedNodes.Current.Value ?? (ListOfVisitedNodes.Current.Value = new ListOfVisitedNodes());

            var firstVisit = !currentListOfVisitedNodes.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentListOfVisitedNodes.VisitedNodes.Add(nodeId);

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
            var reentrantItemTakeResult = collectionForReentrantItems.TryTakeImmediatlyOrReturnNull();

            if (reentrantItemTakeResult != null && reentrantItemTakeResult.Success)
            {
                return reentrantItemTakeResult.Item;
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