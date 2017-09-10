using System;
using System.Threading.Tasks;

namespace ProceduralDataflow.Interfaces
{
    public interface IAsyncProcDataflowBlock
    {
        DfTask Run(Func<Task> action);

        DfTask<TResult> Run<TResult>(Func<Task<TResult>> function);
    }
}