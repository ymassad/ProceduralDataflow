using System;

namespace ProceduralDataflow.Interfaces
{
    public interface IActionRunner
    {
        DfTask Run(Action action);

        DfTask<TResult> Run<TResult>(Func<TResult> function);
    }
}