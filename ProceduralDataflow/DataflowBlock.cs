using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class DataflowBlock : IDataflowBlock, IStartStopable
    {
        private readonly IActionRunner actionRunner;
        private readonly int maximumDegreeOfParallelism;

        private readonly BlockingCollection<Action> collection;

        private readonly BlockingCollection<Action> collectionForReentrantItems;

        private readonly Guid nodeId;

        public DataflowBlock(IActionRunner actionRunner, int maximumNumberOfActionsInQueue, int maximumDegreeOfParallelism)
        {
            this.actionRunner = actionRunner;
            this.maximumDegreeOfParallelism = maximumDegreeOfParallelism;

            collection = new BlockingCollection<Action>(new ConcurrentQueue<Action>(), maximumNumberOfActionsInQueue);

            collectionForReentrantItems = new BlockingCollection<Action>();

            nodeId = Guid.NewGuid();
        }

        public DfTask Run(Action action)
        {
            var task = new DfTask();

            var currentItem = TrackingObject.CurrentProcessingItem.Value ?? (TrackingObject.CurrentProcessingItem.Value = new TrackingObject());

            var firstVisit = !currentItem.VisitedNodes.Contains(nodeId);

            if (firstVisit)
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

        private Thread thread;

        public void Start()
        {
            thread = new Thread(DoIt);

            thread.Start();
        }

        private void DoIt()
        {
            List<WaitHandle> waitHandles = new List<WaitHandle>();

            while (true)
            {
                var action = GetAction();

                if (action == null)
                    return;

                var waitHandle = actionRunner.EnqueueAction(action);

                waitHandles.Add(waitHandle);

                if (waitHandles.Count == maximumDegreeOfParallelism)
                {
                    int index =  WaitHandle.WaitAny(waitHandles.ToArray());

                    waitHandles.RemoveAt(index);
                }
            }
        }

        private Action GetAction()
        {
            if (collectionForReentrantItems.TryTake(out var reentrantItem))
            {
                return reentrantItem;
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