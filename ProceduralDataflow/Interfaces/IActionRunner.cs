using System;
using System.Threading;

namespace ProceduralDataflow.Interfaces
{
    public interface IActionRunner
    {
        WaitHandle EnqueueAction(Action action);
    }
}