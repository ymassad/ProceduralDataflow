using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralDataflow
{
    public static class EnumerableProcessor
    {
        public static async Task ProcessEnumerable<TInput>(
            IEnumerable<TInput> enumerable,
            Func<TInput, Task> action,
            int maximumNumberOfNotCompletedTasks,
            CancellationToken cancellationToken = default)
        {
            List<Task> tasks = new List<Task>();

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
                    await tasks[tasks.Count - 1];

                    tasks.RemoveAt(tasks.Count - 1);
                }
            }




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
