using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class AsyncProcDataflowBlock : IAsyncProcDataflowBlock, IStartStopable
    {
        private readonly int? maximumDegreeOfParallelism;
        private readonly CancellationToken cancellationToken;

        private readonly Channel<Func<Task>> collection;

        private readonly Channel<Func<Task>> collectionForReentrantItems;

        private readonly Guid nodeId;
        
        public AsyncProcDataflowBlock(
            int maximumNumberOfActionsInQueue,
            int? maximumDegreeOfParallelism,
            CancellationToken cancellationToken = default)
        {
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;
            this.cancellationToken = cancellationToken;

            collection = Channel.CreateBounded<Func<Task>>(maximumNumberOfActionsInQueue);

            collectionForReentrantItems = Channel.CreateUnbounded<Func<Task>>();

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
                    if (cancellationToken.IsCancellationRequested)
                    {
                        exception = new OperationCanceledException(cancellationToken);
                    }
                    else
                    {
                        await action();
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
                DfTask.AsyncBlockingTask = collection.Writer.WriteAsync(actionToAddToCollection).AsTask();
            }
            else
            {
                DfTask.AsyncBlockingTask = collectionForReentrantItems.Writer.WriteAsync(actionToAddToCollection).AsTask();
            }

            return task;
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
                        result = await function();
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
                DfTask.AsyncBlockingTask = collection.Writer.WriteAsync(actionToAddToCollection).AsTask();
            }
            else
            {
                DfTask.AsyncBlockingTask = collectionForReentrantItems.Writer.WriteAsync(actionToAddToCollection).AsTask();
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
            collection.Writer.TryComplete();
            collectionForReentrantItems.Writer.TryComplete();
        }

        public static AsyncProcDataflowBlock StartDefault(
            int maximumNumberOfActionsInQueue,
            int? maximumDegreeOfParallelism)
        {
            var block =
                new AsyncProcDataflowBlock(
                    maximumNumberOfActionsInQueue,
                    maximumDegreeOfParallelism);

            block.Start();

            return block;
        }
    }
}