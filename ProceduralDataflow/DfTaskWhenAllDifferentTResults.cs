
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Nito.AsyncEx;

namespace ProceduralDataflow
{

	public partial class DfTask
	{
		public static DfTask<TResult> WhenAll<TResult,T1, T2>(DfTask<T1> task1, DfTask<T2> task2, Func<T1, T2,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 2;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, Func<T1, T2, T3,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 3;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, Func<T1, T2, T3, T4,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 4;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4, T5>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, DfTask<T5> task5, Func<T1, T2, T3, T4, T5,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 5;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();
			T5 task5Result = default(T5);

			TaskCompletionSource tcs5 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs5.SetResult();
                    }
                }
            });

            task5.OnCompleted(() =>
            {
                try
                {
                    task5Result = task5.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs5.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4, T5, T6>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, DfTask<T5> task5, DfTask<T6> task6, Func<T1, T2, T3, T4, T5, T6,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 6;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();
			T5 task5Result = default(T5);

			TaskCompletionSource tcs5 = new TaskCompletionSource();
			T6 task6Result = default(T6);

			TaskCompletionSource tcs6 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                    }
                }
            });

            task5.OnCompleted(() =>
            {
                try
                {
                    task5Result = task5.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs5.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs6.SetResult();
                    }
                }
            });

            task6.OnCompleted(() =>
            {
                try
                {
                    task6Result = task6.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs6.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4, T5, T6, T7>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, DfTask<T5> task5, DfTask<T6> task6, DfTask<T7> task7, Func<T1, T2, T3, T4, T5, T6, T7,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 7;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();
			T5 task5Result = default(T5);

			TaskCompletionSource tcs5 = new TaskCompletionSource();
			T6 task6Result = default(T6);

			TaskCompletionSource tcs6 = new TaskCompletionSource();
			T7 task7Result = default(T7);

			TaskCompletionSource tcs7 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                    }
                }
            });

            task5.OnCompleted(() =>
            {
                try
                {
                    task5Result = task5.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs5.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                    }
                }
            });

            task6.OnCompleted(() =>
            {
                try
                {
                    task6Result = task6.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs6.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs7.SetResult();
                    }
                }
            });

            task7.OnCompleted(() =>
            {
                try
                {
                    task7Result = task7.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs7.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4, T5, T6, T7, T8>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, DfTask<T5> task5, DfTask<T6> task6, DfTask<T7> task7, DfTask<T8> task8, Func<T1, T2, T3, T4, T5, T6, T7, T8,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 8;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();
			T5 task5Result = default(T5);

			TaskCompletionSource tcs5 = new TaskCompletionSource();
			T6 task6Result = default(T6);

			TaskCompletionSource tcs6 = new TaskCompletionSource();
			T7 task7Result = default(T7);

			TaskCompletionSource tcs7 = new TaskCompletionSource();
			T8 task8Result = default(T8);

			TaskCompletionSource tcs8 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task5.OnCompleted(() =>
            {
                try
                {
                    task5Result = task5.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs5.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task6.OnCompleted(() =>
            {
                try
                {
                    task6Result = task6.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs6.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task7.OnCompleted(() =>
            {
                try
                {
                    task7Result = task7.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs7.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs8.SetResult();
                    }
                }
            });

            task8.OnCompleted(() =>
            {
                try
                {
                    task8Result = task8.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs8.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4, T5, T6, T7, T8, T9>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, DfTask<T5> task5, DfTask<T6> task6, DfTask<T7> task7, DfTask<T8> task8, DfTask<T9> task9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 9;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();
			T5 task5Result = default(T5);

			TaskCompletionSource tcs5 = new TaskCompletionSource();
			T6 task6Result = default(T6);

			TaskCompletionSource tcs6 = new TaskCompletionSource();
			T7 task7Result = default(T7);

			TaskCompletionSource tcs7 = new TaskCompletionSource();
			T8 task8Result = default(T8);

			TaskCompletionSource tcs8 = new TaskCompletionSource();
			T9 task9Result = default(T9);

			TaskCompletionSource tcs9 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task5.OnCompleted(() =>
            {
                try
                {
                    task5Result = task5.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs5.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task6.OnCompleted(() =>
            {
                try
                {
                    task6Result = task6.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs6.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task7.OnCompleted(() =>
            {
                try
                {
                    task7Result = task7.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs7.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task8.OnCompleted(() =>
            {
                try
                {
                    task8Result = task8.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs8.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs9.SetResult();
                    }
                }
            });

            task9.OnCompleted(() =>
            {
                try
                {
                    task9Result = task9.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs9.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                    }
                }
            });
            return resultTask;
        }
		public static DfTask<TResult> WhenAll<TResult,T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(DfTask<T1> task1, DfTask<T2> task2, DfTask<T3> task3, DfTask<T4> task4, DfTask<T5> task5, DfTask<T6> task6, DfTask<T7> task7, DfTask<T8> task8, DfTask<T9> task9, DfTask<T10> task10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10,TResult> resultCombiner)
        {
            DfTask<TResult> resultTask = new DfTask<TResult>();

            long totalCompleted = 0;

            long totalShouldComplete = 10;

			T1 task1Result = default(T1);

			TaskCompletionSource tcs1 = new TaskCompletionSource();
			T2 task2Result = default(T2);

			TaskCompletionSource tcs2 = new TaskCompletionSource();
			T3 task3Result = default(T3);

			TaskCompletionSource tcs3 = new TaskCompletionSource();
			T4 task4Result = default(T4);

			TaskCompletionSource tcs4 = new TaskCompletionSource();
			T5 task5Result = default(T5);

			TaskCompletionSource tcs5 = new TaskCompletionSource();
			T6 task6Result = default(T6);

			TaskCompletionSource tcs6 = new TaskCompletionSource();
			T7 task7Result = default(T7);

			TaskCompletionSource tcs7 = new TaskCompletionSource();
			T8 task8Result = default(T8);

			TaskCompletionSource tcs8 = new TaskCompletionSource();
			T9 task9Result = default(T9);

			TaskCompletionSource tcs9 = new TaskCompletionSource();
			T10 task10Result = default(T10);

			TaskCompletionSource tcs10 = new TaskCompletionSource();

            ConcurrentQueue<Exception> errors = new ConcurrentQueue<Exception>();


            task1.OnCompleted(() =>
            {
                try
                {
                    task1Result = task1.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs1.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task2.OnCompleted(() =>
            {
                try
                {
                    task2Result = task2.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs2.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task3.OnCompleted(() =>
            {
                try
                {
                    task3Result = task3.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs3.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task4.OnCompleted(() =>
            {
                try
                {
                    task4Result = task4.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs4.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task5.OnCompleted(() =>
            {
                try
                {
                    task5Result = task5.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs5.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task6.OnCompleted(() =>
            {
                try
                {
                    task6Result = task6.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs6.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task7.OnCompleted(() =>
            {
                try
                {
                    task7Result = task7.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs7.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task8.OnCompleted(() =>
            {
                try
                {
                    task8Result = task8.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs8.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs9.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task9.OnCompleted(() =>
            {
                try
                {
                    task9Result = task9.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs9.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs10.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs10.SetResult();
                    }
                }
            });

            task10.OnCompleted(() =>
            {
                try
                {
                    task10Result = task10.GetResult();
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }

                if (Interlocked.Increment(ref totalCompleted) != totalShouldComplete)
                {
                    AsyncBlockingTask = tcs10.Task;
                }
                else
                {
                    AsyncBlockingTask = null;

                    if (errors.Any())
                        resultTask.SetException(new AggregateException(errors.ToArray()));
                    else
                        resultTask.SetResult(resultCombiner(task1Result, task2Result, task3Result, task4Result, task5Result, task6Result, task7Result, task8Result, task9Result, task10Result));

                    if (AsyncBlockingTask != null)
                    {
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs1.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs2.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs3.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs4.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs5.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs6.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs7.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs8.TryCompleteFromCompletedTask(t));
                        AsyncBlockingTask.ContinueWith(t =>
                            tcs9.TryCompleteFromCompletedTask(t));
                    }
                    else
                    {
                        tcs1.SetResult();
                        tcs2.SetResult();
                        tcs3.SetResult();
                        tcs4.SetResult();
                        tcs5.SetResult();
                        tcs6.SetResult();
                        tcs7.SetResult();
                        tcs8.SetResult();
                        tcs9.SetResult();
                    }
                }
            });
            return resultTask;
        }

	}
}