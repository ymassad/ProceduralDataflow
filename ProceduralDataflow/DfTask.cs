using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralDataflow
{
    [AsyncMethodBuilder(typeof(DfTaskMethodBuilder))]
    public partial class DfTask : INotifyCompletion
    {
        [ThreadStatic]
        public static int AllowCompleteWithoutAwait;

        private Action continuationAction;

        private readonly ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);

        private volatile bool isCompleted;

        private Exception exception;

        public DfTask GetAwaiter() => this;

        public bool IsCompleted => isCompleted;

        public void GetResult()
        {
            if (exception != null)
                throw exception;
        }

        public void SetException(Exception ex)
        {
            exception = ex;

            Complete();
        }

        public void SetResult()
        {
            Complete();
        }

        private void Complete()
        {
            //I am doing this check to support the scenario where DfTask completes synchronously.
            if (DfTask.AllowCompleteWithoutAwait == 0)
            {
                //I am doing this to make sure the continuation is run using the same thread that caused the DfTask to complete
                manualResetEvent.Wait();

                manualResetEvent.Dispose();

                continuationAction();
            }

            isCompleted = true;
        }

        public void OnCompleted(Action continuation)
        {
            continuationAction = continuation;
            manualResetEvent.Set();
        }

        [ThreadStatic]
        public static Task AsyncBlockingTask;
    }
}