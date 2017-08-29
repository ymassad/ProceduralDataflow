using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ProceduralDataflow
{
    [AsyncMethodBuilder(typeof(DfTaskMethodBuilder))]
    public class DfTask : INotifyCompletion
    {
        [ThreadStatic]
        public static bool AllowCompleteWithoutAwait;

        private Action continuationAction;

        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

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
            if (!DfTask.AllowCompleteWithoutAwait)
            {
                manualResetEvent.WaitOne();

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

    [AsyncMethodBuilder(typeof(DfTaskMethodBuilder<>))]
    public class DfTask<TResult> : INotifyCompletion
    {
        private Action continuationAction;

        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

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
            if (!DfTask.AllowCompleteWithoutAwait)
            {
                manualResetEvent.WaitOne();

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