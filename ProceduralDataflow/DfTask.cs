using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace ProceduralDataflow
{
    [AsyncMethodBuilder(typeof(DfTaskMethodBuilder))]
    public class DfTask : INotifyCompletion
    {
        [ThreadStatic]
        public static bool AllowCompleteWithoutAwait;

        private Action continuationAction;

        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private volatile bool isCompleted;

        private Exception exception;

        public DfTask GetAwaiter() => this;

        public bool IsCompleted => isCompleted;

        public void GetResult()
        {
            if (exception != null)
                throw exception;
        }

        public void SetException(Exception ex)
        {
            exception = ex;

            Complete();
        }

        public void SetResult()
        {
            Complete();
        }

        private void Complete()
        {
            if (!DfTask.AllowCompleteWithoutAwait)
            {
                manualResetEvent.WaitOne();

                continuationAction();
            }

            isCompleted = true;
        }

        public void OnCompleted(Action continuation)
        {
            continuationAction = continuation;
            manualResetEvent.Set();
        }

        public static DfTask WhenAll(params DfTask[] tasks)
        {
            DfTask resultTask = new DfTask();

            long totalCompleted = 0;

            long totalShouldComplete = tasks.Length;

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();

            TaskCompletionSource[] taskCompletionSources = new TaskCompletionSource[tasks.Length];

            for (var index = 0; index < tasks.Length; index++)
            {
                var task = tasks[index];

                taskCompletionSources[index] = new TaskCompletionSource();

                int index1 = index;

                task.OnCompleted(() =>
                {
                    try
                    {
                        task.GetResult();
                    }
                    catch (Exception e)
                    {
                        errors.Enqueue(e);
                    }

                    if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                    {
                        AsyncBlockingTask = taskCompletionSources[index1].Task;
                    }
                    else
                    {
                        AsyncBlockingTask = null;

                        if (errors.Count > 0)
                            resultTask.SetException(new AggregateException(errors.ToArray()));
                        else
                            resultTask.SetResult();

                        if (AsyncBlockingTask != null)
                        {
                            for (int i = 0; i < tasks.Length; i++)
                            {
                                int i1 = i;

                                AsyncBlockingTask.ContinueWith(t =>
                                    taskCompletionSources[i1].TryCompleteFromCompletedTask(t));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < tasks.Length; i++)
                            {
                                int i1 = i;

                                taskCompletionSources[i1].SetResult();
                            }
                        }
                    }
                });
            }

            return resultTask;
        }

        public static DfTask<T[]> WhenAll<T>(params DfTask<T>[] tasks)
        {
            DfTask<T[]> resultTask = new DfTask<T[]>();

            long totalCompleted = 0;

            long totalShouldComplete = tasks.Length;

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();

            ConcurrentQueue<(int index, T value)> results = new ConcurrentQueue<(int index, T value)>();

            TaskCompletionSource[] taskCompletionSources = new TaskCompletionSource[tasks.Length];

            for (var index = 0; index < tasks.Length; index++)
            {
                var task = tasks[index];

                taskCompletionSources[index] = new TaskCompletionSource();

                int index1 = index;

                task.OnCompleted(() =>
                {
                    try
                    {
                        results.Enqueue((index1, task.GetResult()));
                    }
                    catch (Exception e)
                    {
                        errors.Enqueue(e);
                    }

                    if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                    {
                        AsyncBlockingTask = taskCompletionSources[index1].Task;
                    }
                    else
                    {
                        AsyncBlockingTask = null;

                        if (errors.Count > 0)
                            resultTask.SetException(new AggregateException(errors.ToArray()));
                        else
                            resultTask.SetResult(results.OrderBy(x => x.index).Select(x => x.value).ToArray());

                        if (AsyncBlockingTask != null)
                        {
                            for (int i = 0; i < tasks.Length; i++)
                            {
                                int i1 = i;

                                AsyncBlockingTask.ContinueWith(t =>
                                    taskCompletionSources[i1].TryCompleteFromCompletedTask(t));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < tasks.Length; i++)
                            {
                                int i1 = i;

                                taskCompletionSources[i1].SetResult();
                            }
                        }
                    }
                });
            }

            return resultTask;
        }

        [ThreadStatic]
        public static Task AsyncBlockingTask;
    }

    [AsyncMethodBuilder(typeof(DfTaskMethodBuilder<>))]
    public class DfTask<TResult> : INotifyCompletion
    {
        private Action continuationAction;

        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private volatile bool isCompleted;

        private Exception exception;

        private TResult result;

        public DfTask<TResult> GetAwaiter() => this;

        public bool IsCompleted => isCompleted;

        public TResult GetResult()
        {
            if (exception != null)
                throw exception;

            return result;
        }

        public void SetException(Exception ex)
        {
            exception = ex;

            Complete();
        }

        public void SetResult(TResult value)
        {
            result = value;

            Complete();
        }

        private void Complete()
        {
            if (!DfTask.AllowCompleteWithoutAwait)
            {
                manualResetEvent.WaitOne();

                continuationAction();
            }

            isCompleted = true;
        }

        public void OnCompleted(Action continuation)
        {
            continuationAction = continuation;
            manualResetEvent.Set();
        }
    }
}