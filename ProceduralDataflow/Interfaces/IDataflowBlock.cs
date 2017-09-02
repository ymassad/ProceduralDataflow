using System;

namespace ProceduralDataflow.Interfaces
{
    public interface IDataflowBlock
    {
        DfTask Run(Action action);

        DfTask<TResult> Run<TResult>(Func<TResult> function);
    }
}