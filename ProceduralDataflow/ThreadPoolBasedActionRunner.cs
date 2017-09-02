using System;
using System.Threading;
using System.Threading.Tasks;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow
{
    public class ThreadPoolBasedActionRunner : IActionRunner
    {
        public WaitHandle EnqueueAction(Action action)
        {
            ManualResetEvent handle = new ManualResetEvent(false);

            Task.Run(() =>
            {
                action();

                handle.Set();
            });

            return handle;
        }
    }
}