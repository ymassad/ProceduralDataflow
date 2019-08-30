using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralDataflow
{
    public static class EnumerableProcessor
    {
        public static async Task<ProcessEnumerableResult> ProcessEnumerable<TInput>(
            IEnumerable<TInput> enumerable,
            Func<TInput, Task> action,
            int maximumNumberOfNotCompletedTasks,
            CancellationToken cancellationToken = default)
        {
            var result = await ProcessEnumerable_Internal(
                enumerable,
                action,
                (object) null,
                (x, _) => null,
                _ => (object) null,
                maximumNumberOfNotCompletedTasks,
                cancellationToken);

            return new ProcessEnumerableResult(result.CancelledTasks, result.FaultedTasks);
        }

        private static async Task<ProcessEnumerableResult<TResult>> ProcessEnumerable_Internal<TInput, TOutput, TResult>(
            IEnumerable<TInput> enumerable,
            Func<TInput, Task> action,
            TResult seed,
            Func<TResult, TOutput, TResult> accumulator,
            Func<Task, TOutput> getOutput,
            int maximumNumberOfNotCompletedTasks,
            CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();

            var cancelledTasks = new List<Task>();

            var faultedTasks = new List<Task>();

            TResult result = seed;

            try
            {
                foreach (var dataItem in enumerable)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var task = action(dataItem);

                    tasks.Add(task);

                    if (tasks.Count == maximumNumberOfNotCompletedTasks)
                    {
                        var removedTask = await Task.WhenAny(tasks);

                        if (removedTask.IsCanceled)
                            cancelledTasks.Add(removedTask);
                        else if (removedTask.IsFaulted)
                            faultedTasks.Add(removedTask);
                        else
                        {
                            result = accumulator(result, getOutput(removedTask));
                        }
                        tasks.Remove(removedTask);
                    }
                }
            }
            finally
            {
                while (tasks.Count > 0)
                {
                    var task = tasks[tasks.Count - 1];
                    try
                    {
                        await task;
                    }
                    catch
                    {
                        
                    }

                    if (task.IsCanceled)
                        cancelledTasks.Add(task);
                    else if (task.IsFaulted)
                        faultedTasks.Add(task);
                    else
                    {
                        result = accumulator(result, getOutput(task));
                    }

                    tasks.RemoveAt(tasks.Count - 1);
                }
            }

            return new ProcessEnumerableResult<TResult>(result, cancelledTasks.ToImmutableArray(), faultedTasks.ToImmutableArray());
        }

        public static Task<ProcessEnumerableResult<TResult>> ProcessEnumerable<TInput,TOutput,TResult>(
            IEnumerable<TInput> enumerable,
            Func<TInput, Task<TOutput>> action,
            TResult seed,
            Func<TResult,TOutput,TResult> accumulator,
            int maximumNumberOfNotCompletedTasks,
            CancellationToken cancellationToken = default)
        {
            return ProcessEnumerable_Internal(enumerable, action, seed, accumulator, x => ((Task<TOutput>) x).Result, maximumNumberOfNotCompletedTasks, cancellationToken);
        }
    }
}
