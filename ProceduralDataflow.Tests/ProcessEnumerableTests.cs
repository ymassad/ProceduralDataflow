using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProceduralDataflow.Tests
{
    [TestClass]
    public class ProcessEnumerableTests
    {
        [TestMethod]
        public async Task BasicTest()
        {
            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
            }

            ConcurrentQueue<int> processedData = new ConcurrentQueue<int>();

            async Task GetTask(int data)
            {
                processedData.Enqueue(data);
            }

            await EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, 5);

            processedData.Should().ContainInOrder(new[] {1, 2});
        }

        [TestMethod]
        public async Task TestMaximumNumberOfNotCompletedTasks()
        {
            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<object>>();

            Task GetTask(int data)
            {
                var tcs = new TaskCompletionSource<object>();

                taskCompletionSources.Add(data, tcs);

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, 2);

            WaitUntil(() => taskCompletionSources.Count == 2, TimeSpan.FromSeconds(1));


            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(2);

            taskCompletionSources[1].SetResult(null);

            WaitUntil(() => taskCompletionSources.Count == 3, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            task.IsCompleted.Should().BeFalse();
        }

        [TestMethod]
        public async Task TestCancellation()
        {

            CancellationTokenSource cts = new CancellationTokenSource();

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            ConcurrentQueue<int> processedData = new ConcurrentQueue<int>();

            async Task GetTask(int data)
            {
                processedData.Enqueue(data);

                if(data == 3)
                    cts.Cancel();
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, 5, cts.Token);

            WaitUntil(() => processedData.Count == 3, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            processedData.Should().ContainInOrder(new[] {1, 2, 3});

            task.IsCanceled.Should().BeTrue();

            new Action(() => task.Wait()).ShouldThrow<AggregateException>()
                .Where(x => x.InnerExceptions.Count == 1)
                .Where(x => x.InnerExceptions[0] is OperationCanceledException);
        }

        [TestMethod]
        public async Task TestCancellation_ShouldWaitForPreviousTasks()
        {

            CancellationTokenSource cts = new CancellationTokenSource();

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<object>>();


            Task GetTask(int data)
            {
                var tcs = new TaskCompletionSource<object>();

                taskCompletionSources.Add(data, tcs);

                if (data == 3)
                    cts.Cancel();

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, 5, cts.Token);

            WaitUntil(() => taskCompletionSources.Count == 3, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            taskCompletionSources.Keys.Should().Contain(new[] { 1, 2, 3 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();

            taskCompletionSources[1].SetResult(null);
            taskCompletionSources[2].SetResult(null);
            taskCompletionSources[3].SetResult(null);

            WaitUntil(() => task.IsCanceled, TimeSpan.FromSeconds(1));

            new Action(() => task.Wait()).ShouldThrow<AggregateException>()
                .Where(x => x.InnerExceptions.Count == 1)
                .Where(x => x.InnerExceptions[0] is OperationCanceledException);
        }


        void WaitUntil(Func<bool> func, TimeSpan timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (true)
            {

                if (sw.Elapsed > timeout)
                {
                    throw new Exception("Timeout waiting");
                }

                if (func())
                    return;

                Thread.Sleep(50);
            }
        }
    }
}
