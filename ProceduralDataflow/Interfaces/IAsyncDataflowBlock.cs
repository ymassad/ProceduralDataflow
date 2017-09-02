using System;
using System.Threading.Tasks;

namespace ProceduralDataflow.Interfaces
{
    public interface IAsyncDataflowBlock
    {
        DfTask Run(Func<Task> action);

        DfTask<TResult> Run<TResult>(Func<Task<TResult>> function);
    }
}