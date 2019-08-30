using System.Collections.Immutable;
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

    public sealed class ProcessEnumerableResult<TResult>
    {
        public TResult Result { get; }

        public ImmutableArray<Task> CancelledTasks { get; }

        public ImmutableArray<Task> FaultedTasks { get; }

        public ProcessEnumerableResult(TResult result, ImmutableArray<Task> cancelledTasks, ImmutableArray<Task> faultedTasks)
        {
            Result = result;
            CancelledTasks = cancelledTasks;
            FaultedTasks = faultedTasks;
        }
    }
}