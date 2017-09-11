using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace ProceduralDataflow
{
    public partial class DfTask
    {
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
    }
}
