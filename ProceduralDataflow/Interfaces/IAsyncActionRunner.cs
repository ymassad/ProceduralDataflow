using System;
using System.Threading.Tasks;

namespace ProceduralDataflow.Interfaces
{
    public interface IAsyncActionRunner
    {
        DfTask Run(Func<Task> action);

        DfTask<TResult> Run<TResult>(Func<Task<TResult>> function);
    }
}