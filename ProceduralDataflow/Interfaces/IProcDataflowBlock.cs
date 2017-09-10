using System;

namespace ProceduralDataflow.Interfaces
{
    public interface IProcDataflowBlock
    {
        DfTask Run(Action action);

        DfTask<TResult> Run<TResult>(Func<TResult> function);
    }
}