using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralDataflow
{

    public sealed class ProcessEnumerableResult
    {
        public ImmutableArray<Task> CancelledTasks { get; }

        public ImmutableArray<Task> FaultedTasks { get; }

        public ProcessEnumerableResult(ImmutableArray<Task> cancelledTasks, ImmutableArray<Task> faultedTasks)
        {
            CancelledTasks = cancelledTasks;
            FaultedTasks = faultedTasks;
        }
    }


    public static class EnumerableProcessor
    {
        public static async Task<ProcessEnumerableResult> ProcessEnumerable<TInput>(
            IEnumerable<TInput> enumerable,
            Func<TInput, Task> action,
            int maximumNumberOfNotCompletedTasks,
            CancellationToken cancellationToken = default)
        {
            List<Task> tasks = new List<Task>();
            

            var cancelledTasks = new List<Task>();

            var faultedTasks = new List<Task>();

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
                    catch (Exception e)
                    {
                        
                    }

    
                    if(task.IsCanceled)
                        cancelledTasks.Add(task);
                    else if (task.IsFaulted)
                        faultedTasks.Add(task);

                    tasks.RemoveAt(tasks.Count - 1);
                }
            }

            return new ProcessEnumerableResult(cancelledTasks.ToImmutableArray(), faultedTasks.ToImmutableArray());


        }

        public static async Task<TResult> ProcessEnumerable<TInput,TOutput,TResult>(
            IEnumerable<TInput> enumerable,
            Func<TInput, Task<TOutput>> action,
            TResult seed,
            Func<TResult,TOutput,TResult> accumulator,
            int maximumNumberOfNotCompletedTasks,
            CancellationToken cancellationToken = default)
        {
            List<Task<TOutput>> tasks = new List<Task<TOutput>>();

            TResult result = seed;

            foreach (var dataItem in enumerable)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var task = action(dataItem);

                tasks.Add(task);

                if (tasks.Count == maximumNumberOfNotCompletedTasks)
                {
                    var removedTask = await Task.WhenAny(tasks);

                    tasks.Remove(removedTask);

                    result = accumulator(result, removedTask.Result);
                }
            }

            while (tasks.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var output = await tasks[tasks.Count - 1];

                result = accumulator(result, output);

                tasks.RemoveAt(tasks.Count - 1);
            }

            return result;
        }
    }
}
