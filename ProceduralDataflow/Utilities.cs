using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace ProceduralDataflow
{
    public static class Utilities
    {
        public static async Task<T> TakeItemFromAnyCollectionWithPriorityToFirstCollection<T>(
            AsyncCollection<T> collection1,
            AsyncCollection<T> collection2)
            where T:class
        {
            var reentrantItemTakeResult = collection1.TryTakeImmediatlyOrReturnNull();

            if (reentrantItemTakeResult != null && reentrantItemTakeResult.Success)
            {
                return reentrantItemTakeResult.Item;
            }

            var itemResult = await new[] {collection1, collection2}.TryTakeFromAnyAsync();

            if (!itemResult.Success)
                return null;

            return itemResult.Item;
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
    }
}
