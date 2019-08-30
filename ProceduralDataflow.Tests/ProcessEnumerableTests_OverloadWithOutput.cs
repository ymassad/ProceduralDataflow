﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProceduralDataflow.Tests
{
    [TestClass]
    public class ProcessEnumerableTests_OverloadWithOutput
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

            async Task<string> GetTask(int data)
            {
                processedData.Enqueue(data);

                return data.ToString();
            }

            var result = await EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x,y) => x.Add(y) , 5);

            processedData.SequenceEqual(new[] {1, 2}).Should().BeTrue();

            result.CancelledTasks.Should().BeEmpty();

            result.FaultedTasks.Should().BeEmpty();

            result.Result.Should().BeEquivalentTo(new[] {"1", "2"});
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

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();

            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 2);

            WaitUntil(() => taskCompletionSources.Count == 2, TimeSpan.FromSeconds(1));


            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(2);

            taskCompletionSources[1].SetResult("1");

            WaitUntil(() => taskCompletionSources.Count == 3, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            task.IsCompleted.Should().BeFalse();

            taskCompletionSources[2].SetResult("2");
            taskCompletionSources[3].SetResult("3");
            taskCompletionSources[4].SetResult("4");
            taskCompletionSources[5].SetResult("5");

            var result = await task;

            result.CancelledTasks.Should().BeEmpty();

            result.FaultedTasks.Should().BeEmpty();

            result.Result.Should().BeEquivalentTo(new[] {"1", "2", "3", "4", "5"});
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

            async Task<string> GetTask(int data)
            {
                processedData.Enqueue(data);

                if (data == 3)
                    cts.Cancel();

                return data.ToString();
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x,y) => x.Add(y), 5, cts.Token);

            WaitUntil(() => processedData.Count == 3, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            processedData.SequenceEqual(new[] { 1, 2, 3 }).Should().BeTrue();

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

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();


            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                if (data == 3)
                    cts.Cancel();

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 5, cts.Token);

            WaitUntil(() => taskCompletionSources.Count == 3, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();

            taskCompletionSources[1].SetResult("1");
            taskCompletionSources[2].SetResult("2");
            taskCompletionSources[3].SetResult("3");

            WaitUntil(() => task.IsCanceled, TimeSpan.FromSeconds(1));

            new Action(() => task.Wait()).ShouldThrow<AggregateException>()
                .Where(x => x.InnerExceptions.Count == 1)
                .Where(x => x.InnerExceptions[0] is OperationCanceledException);
        }

        [TestMethod]
        public async Task Test_Task1And2CompleteTask3Fails_ShouldWaitForTask4And5()
        {

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();


            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                if (data == 1 || data == 2)
                    tcs.SetResult(data.ToString());

                if (data == 3)
                    tcs.SetException(new MyCustomException("My exception"));

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 10);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(5);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[4].SetResult("4");
            taskCompletionSources[5].SetResult("5");

            var result = await task;

            result.CancelledTasks.Should().BeEmpty();
            result.FaultedTasks.Length.Should().Be(1);

            result.FaultedTasks[0].IsFaulted.Should().BeTrue();

            result.FaultedTasks[0].Exception.InnerExceptions.Count.Should().Be(1);
            result.FaultedTasks[0].Exception.InnerExceptions[0].Should().BeOfType<MyCustomException>()
                .Which.Message.Should().Be("My exception");

            result.Result.Should().BeEquivalentTo(new[] {"1", "2", "4", "5"});
        }


        [TestMethod]
        public async Task Test_Task4And5CompleteTask3Fails_ShouldWaitForTask1And2()
        {

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();


            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                if (data == 4 || data == 5)
                    tcs.SetResult(data.ToString());

                if (data == 3)
                    tcs.SetException(new MyCustomException("My exception"));

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 10);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(5);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[1].SetResult("1");
            taskCompletionSources[2].SetResult("2");

            var result = await task;

            result.CancelledTasks.Should().BeEmpty();
            result.FaultedTasks.Length.Should().Be(1);

            result.FaultedTasks[0].IsFaulted.Should().BeTrue();

            result.FaultedTasks[0].Exception.InnerExceptions.Count.Should().Be(1);
            result.FaultedTasks[0].Exception.InnerExceptions[0].Should().BeOfType<MyCustomException>()
                .Which.Message.Should().Be("My exception");

            result.Result.Should().BeEquivalentTo(new[] { "1", "2", "4", "5" });

        }

        [TestMethod]
        public async Task Test_Task5CompletesTask3And4Fail_ShouldWaitForTask1And2_AndResultShouldContainTwoFaultedTasks()
        {

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();

            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                if (data == 5)
                    tcs.SetResult("5");

                if (data == 3 || data == 4)
                    tcs.SetException(new MyCustomException("My exception " + data));

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 10);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(5);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[1].SetResult("1");
            taskCompletionSources[2].SetResult("2");

            var result = await task;

            result.CancelledTasks.Should().BeEmpty();
            result.FaultedTasks.Length.Should().Be(2);

            result.FaultedTasks.All(x => x.IsFaulted).Should().BeTrue();

            result.FaultedTasks.All(x => x.Exception.InnerExceptions.Count == 1).Should().BeTrue();

            result.FaultedTasks.All(x => x.Exception.InnerExceptions[0] is MyCustomException).Should().BeTrue();

            result.FaultedTasks.Any(x => x.Exception.InnerExceptions[0].Message == "My exception 3").Should().BeTrue();
            result.FaultedTasks.Any(x => x.Exception.InnerExceptions[0].Message == "My exception 4").Should().BeTrue();

            result.Result.Should().BeEquivalentTo(new[] { "1", "2", "5" });
        }

        [TestMethod]
        public async Task Test_Task1And2CompleteTask3And4Fail_ShouldWaitForTask5_AndResultShouldContainTwoFaultedTasks()
        {

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();

            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                if (data == 1 || data == 2)
                    tcs.SetResult(data.ToString());

                if (data == 3 || data == 4)
                    tcs.SetException(new MyCustomException("My exception " + data));

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 10);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(5);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[5].SetResult("5");

            var result = await task;

            result.CancelledTasks.Should().BeEmpty();
            result.FaultedTasks.Length.Should().Be(2);

            result.FaultedTasks.All(x => x.IsFaulted).Should().BeTrue();

            result.FaultedTasks.All(x => x.Exception.InnerExceptions.Count == 1).Should().BeTrue();

            result.FaultedTasks.All(x => x.Exception.InnerExceptions[0] is MyCustomException).Should().BeTrue();

            result.FaultedTasks.Any(x => x.Exception.InnerExceptions[0].Message == "My exception 3").Should().BeTrue();
            result.FaultedTasks.Any(x => x.Exception.InnerExceptions[0].Message == "My exception 4").Should().BeTrue();

            result.Result.Should().BeEquivalentTo(new[] { "1", "2", "5" });
        }

        [TestMethod]
        public async Task Test_Task1And2CompleteTask3IsCancelled_ShouldWaitForTask4And5()
        {

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();


            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                if (data == 1 || data == 2)
                    tcs.SetResult(data.ToString());

                if (data == 3)
                    tcs.SetCanceled();

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 10);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(5);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[4].SetResult("4");
            taskCompletionSources[5].SetResult("5");

            var result = await task;

            result.FaultedTasks.Should().BeEmpty();
            result.CancelledTasks.Length.Should().Be(1);

            result.CancelledTasks[0].IsCanceled.Should().BeTrue();

            result.Result.Should().BeEquivalentTo(new[] { "1", "2", "4", "5" });
        }

        [TestMethod]
        public async Task Test_MaximumNumberOfNotCompletedTasksIs1_Task3FailsLater_ResultContainsTheFaultedTask3()
        {

            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();


            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 1);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(1);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[1].SetResult("1");

            taskCompletionSources[2].SetResult("2");

            Thread.Sleep(500);

            taskCompletionSources[3].SetException(new MyCustomException("My exception"));

            Thread.Sleep(500);

            taskCompletionSources[4].SetResult("4");

            taskCompletionSources[5].SetResult("5");

            var result = await task;

            result.CancelledTasks.Should().BeEmpty();
            result.FaultedTasks.Length.Should().Be(1);

            result.FaultedTasks[0].IsFaulted.Should().BeTrue();

            result.FaultedTasks[0].Exception.InnerExceptions.Count.Should().Be(1);
            result.FaultedTasks[0].Exception.InnerExceptions[0].Should().BeOfType<MyCustomException>()
                .Which.Message.Should().Be("My exception");

            result.Result.Should().BeEquivalentTo(new[] { "1", "2", "4", "5" });
        }

        [TestMethod]
        public async Task Test_MaximumNumberOfNotCompletedTasksIs1_Task3IsCancelledLater_ResultContainsTheCancelledTask3()
        {
            IEnumerable<int> GetData()
            {
                yield return 1;
                yield return 2;
                yield return 3;
                yield return 4;
                yield return 5;
            }

            var taskCompletionSources = new Dictionary<int, TaskCompletionSource<string>>();


            Task<string> GetTask(int data)
            {
                var tcs = new TaskCompletionSource<string>();

                taskCompletionSources.Add(data, tcs);

                return tcs.Task;
            }

            var task = EnumerableProcessor.ProcessEnumerable(GetData(), GetTask, ImmutableArray<string>.Empty, (x, y) => x.Add(y), 1);

            Thread.Sleep(500);

            taskCompletionSources.Count.Should().Be(1);

            taskCompletionSources.Keys.Should().BeEquivalentTo(new[] { 1 });

            task.IsCanceled.Should().BeFalse();
            task.IsCompleted.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();

            taskCompletionSources[1].SetResult("1");

            taskCompletionSources[2].SetResult("2");

            Thread.Sleep(500);

            taskCompletionSources[3].SetCanceled();

            Thread.Sleep(500);

            taskCompletionSources[4].SetResult("4");

            taskCompletionSources[5].SetResult("5");

            var result = await task;

            result.FaultedTasks.Should().BeEmpty();
            result.CancelledTasks.Length.Should().Be(1);

            result.CancelledTasks[0].IsCanceled.Should().BeTrue();

            result.Result.Should().BeEquivalentTo(new[] { "1", "2", "4", "5" });
        }


        public class MyCustomException : Exception
        {
            public MyCustomException(string message) : base(message)
            {

            }
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