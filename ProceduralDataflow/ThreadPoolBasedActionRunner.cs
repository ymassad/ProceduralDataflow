using System;
using System.Threading;
using System.Threading.Tasks;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class ThreadPoolBasedActionRunner : IActionRunner
    {
        public Task EnqueueAction(Action action)
        {
            return Task.Run(action);
        }
    }
}