using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralDataflow.Interfaces
{
    public interface IActionRunner
    {
        //For CPU-bound operations
        Task EnqueueAction(Action action);

        //For operations that are in part CPU-bound
        Task EnqueueAction(Func<Task> action);
    }
}