using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ProceduralDataflow
{
    [AsyncMethodBuilder(typeof(DfTaskMethodBuilder<>))]
    public class DfTask<TResult> : INotifyCompletion
    {
        private Action continuationAction;

        private readonly ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);

        private volatile bool isCompleted;

        private Exception exception;

        private TResult result;

        public DfTask<TResult> GetAwaiter() => this;

        public bool IsCompleted => isCompleted;

        public TResult GetResult()
        {
            if (exception != null)
                throw exception;

            return result;
        }

        public void SetException(Exception ex)
        {
            exception = ex;

            Complete();
        }

        public void SetResult(TResult value)
        {
            result = value;

            Complete();
        }

        private void Complete()
        {
            if (DfTask.AllowCompleteWithoutAwait == 0)
            {
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
    }
}