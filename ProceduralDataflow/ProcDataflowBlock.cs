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

        private readonly CancellationToken cancellationToken;

        private readonly AsyncCollection<Func<Task>> collection;

        private readonly AsyncCollection<Func<Task>> collectionForReentrantItems;

        private readonly Guid nodeId;

        public ProcDataflowBlock(
            IActionRunner actionRunner,
            int maximumNumberOfActionsInQueue,
            int? maximumDegreeOfParallelism,
            CancellationToken cancellationToken = default)
        {
            this.actionRunner = actionRunner;
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;
            this.cancellationToken = cancellationToken;

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
                    if (cancellationToken.IsCancellationRequested)
                    {
                        exception = new OperationCanceledException(cancellationToken);
                    }
                    else
                    {
                        action();
                    }
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
                Utilities.MakeActionRunInCurrentExecutionContextIfAny(runAction);

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

        public DfTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new DfTask<TResult>();

            var currentListOfVisitedNodes = ListOfVisitedNodes.Current.Value ?? (ListOfVisitedNodes.Current.Value = new ListOfVisitedNodes());

            var firstVisit = !currentListOfVisitedNodes.VisitedNodes.Contains(nodeId);

            if (firstVisit)
                currentListOfVisitedNodes.VisitedNodes.Add(nodeId);

            Func<Task> runAction = async () =>
            {
                TResult result = default;

                Exception exception = null;

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        exception = new OperationCanceledException(cancellationToken);
                    }
                    else
                    {
                        result = function();
                    }
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
                Utilities.MakeActionRunInCurrentExecutionContextIfAny(runAction);

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
                        var taskToRemove = await Task.WhenAny(tasks);

                        tasks.Remove(taskToRemove);
                    }
                }
            }
        }

        private async Task<Func<Task>> GetAction()
        {
            return await Utilities.TakeItemFromAnyCollectionWithPriorityToFirstCollection(collectionForReentrantItems, collection);
        }

        public void Stop()
        {
            collection.CompleteAdding();
            collectionForReentrantItems.CompleteAdding();
        }

        public static ProcDataflowBlock StartDefault(
            int maximumNumberOfActionsInQueue,
            int? maximumDegreeOfParallelism)
        {
            var block =
                new ProcDataflowBlock(
                    new ThreadPoolBasedActionRunner(),
                    maximumNumberOfActionsInQueue,
                    maximumDegreeOfParallelism);

            block.Start();

            return block;
        }
    }
}