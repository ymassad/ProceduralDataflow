using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralDataflow.Interfaces
{
    public interface IActionRunner
    {
        Task EnqueueAction(Action action);
    }
}