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
}