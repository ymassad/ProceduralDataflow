using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace ProceduralDataflow
{
    public class DfTaskMethodBuilder<TResult>
    {
        private readonly DfTask<TResult> task = new DfTask<TResult>();

        private IAsyncStateMachine stateMachine;

        public static DfTaskMethodBuilder<TResult> Create()
        {
            return new DfTaskMethodBuilder<TResult>();
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            DfTask.AllowCompleteWithoutAwait++;

            try
            {
                stateMachine.MoveNext();
            }
            finally
            {
                DfTask.AllowCompleteWithoutAwait--;
            }
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void SetResult(TResult result)
        {
            task.SetResult(result);
        }

        public void SetException(Exception exception)
        {
            task.SetException(exception);
        }

        public DfTask<TResult> Task => task;

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted(awaiter, stateMachine);
        }

        private void AwaitOnCompleted(INotifyCompletion awaiter, IAsyncStateMachine stateMachine)
        {
            awaiter.OnCompleted(() => stateMachine.MoveNext());
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted(awaiter, stateMachine);
        }
    }

    public class DfTaskMethodBuilder
    {
        private readonly DfTask task = new DfTask();

        private IAsyncStateMachine stateMachine;

        public static DfTaskMethodBuilder Create()
        {
            return new DfTaskMethodBuilder();
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            DfTask.AllowCompleteWithoutAwait++;

            try
            {
                stateMachine.MoveNext();
            }
            finally
            {
                DfTask.AllowCompleteWithoutAwait--;
            }
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void SetResult()
        {
            task.SetResult();
        }

        public void SetException(Exception exception)
        {
            task.SetException(exception);
        }

        public DfTask Task => task;

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted(awaiter, stateMachine);
        }

        private void AwaitOnCompleted(INotifyCompletion awaiter, IAsyncStateMachine stateMachine)
        {
            awaiter.OnCompleted(() => stateMachine.MoveNext());
        }

        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted(awaiter, stateMachine);
        }
    }
}