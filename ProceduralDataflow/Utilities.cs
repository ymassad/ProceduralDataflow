using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProceduralDataflow
{
    public static class Utilities
    {
        public static async Task<T> TakeItemFromAnyCollectionWithPriorityToFirstCollection<T>(
            Channel<T> collection1,
            Channel<T> collection2)
            where T:class
        {
            while (true)
            {
                if (collection1.Reader.Completion.IsCompleted && collection2.Reader.Completion.IsCompleted)
                    return null;

                if (collection1.Reader.TryRead(out var item1))
                    return item1;

                var collection1WaitTask = collection1.Reader.WaitToReadAsync().AsTask();

                var collection2WaitTask = collection2.Reader.WaitToReadAsync().AsTask();

                var completedTask = await Task.WhenAny(collection1WaitTask, collection2WaitTask);

                if (ReferenceEquals(completedTask, collection1WaitTask))
                {
                    if (completedTask.Result)
                    {
                        if (collection1.Reader.TryRead(out item1))
                            return item1;
                    }
                }
                else
                {
                    if (completedTask.Result)
                    {
                        if (collection2.Reader.TryRead(out var item2))
                            return item2;
                    }
                }
            }
        }

        public static Func<Task> MakeActionRunInCurrentExecutionContextIfAny(Func<Task> action)
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

        public static bool TryCompleteFromCompletedTask<TResult>(
            this TaskCompletionSource<TResult> taskCompletionSource,
            Task<TResult> completedTask)
        {
            if (completedTask.Status == TaskStatus.Faulted)
                return taskCompletionSource.TrySetException(completedTask.Exception.InnerExceptions);
            if (completedTask.Status == TaskStatus.Canceled)
                return taskCompletionSource.TrySetCanceled();
            if (completedTask.Status == TaskStatus.RanToCompletion)
                return taskCompletionSource.TrySetResult(completedTask.Result);

            throw new Exception("Task is not completed");
        }

        public static bool TryCompleteFromCompletedTask(
            this TaskCompletionSource<object> taskCompletionSource,
            Task completedTask)
        {
            if (completedTask.Status == TaskStatus.Faulted)
                return taskCompletionSource.TrySetException(completedTask.Exception.InnerExceptions);
            if (completedTask.Status == TaskStatus.Canceled)
                return taskCompletionSource.TrySetCanceled();
            if (completedTask.Status == TaskStatus.RanToCompletion)
                return taskCompletionSource.TrySetResult(null);

            throw new Exception("Task is not completed");
        }
    }
}
