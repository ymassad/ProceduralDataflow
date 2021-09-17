using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProceduralDataflow.Interfaces;

namespace ProceduralDataflow.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task AsyncMethodsThatUseTheDfSystemWillComplete()
        {
            await CreateAndUseNewBlock(1, 1, async runner =>
            {
                async Task Method1()
                {
                    await runner.Run(() => { });
                }

                await Method1();
            });
        }

        [TestMethod]
        public async Task CustomThreadsBasedActionRunnerWillActuallyRunTheAction()
        {
            await CreateAndUseNewBlock(1, 1, async runner =>
            {
                bool flag = false;

                async Task Method1()
                {
                    await runner.Run(() => { flag = true; });
                }

                await Method1();

                Assert.IsTrue(flag);
            });
        }

        [TestMethod]
        public async Task FastProducerWillBeSlowedBySlowConsumer()
        {
            await RunBasicTwoOperationsTest(
                numberOfThreadsForFirstOperation: 1,
                queue1Size: 1,
                numberOfThreadsForSecondOperation: 1,
                queye2Size: 1,
                numberOfTimesToProcess: 10,
                expectedNumberOfFirstOperationCompleteForEachTimeOperationTwoIsInvoked:
                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 });
        }

        [TestMethod]
        public async Task TestPauseAndResume()
        {
            var node = new ProcDataflowBlock(new ThreadPoolBasedActionRunner(), 1, 1, default, true);

            node.Start();

            try
            {
                long numberOfTimesOperationWasRun = 0;

                async Task Method1()
                {
                    await node.Run(() =>
                    {
                        var numberOfTimes = SimulateWork(
                            TimeSpan.FromMilliseconds(100), ref numberOfTimesOperationWasRun);

                    });
                }

                var tasks = Enumerable.Range(0, 30).Select(_ => Method1()).ToArray();

                await Task.Delay(1000);

                node.Pause();

                await Task.Delay(1000);

                var times = Interlocked.Read(ref numberOfTimesOperationWasRun);

                await Task.Delay(1000);

                var times2 = Interlocked.Read(ref numberOfTimesOperationWasRun);

                times.Equals(times2).Should().BeTrue();

                times.Should().BeLessThan(30);

                node.Resume();

                await Task.WhenAll(tasks);

                Interlocked.Read(ref numberOfTimesOperationWasRun).Should().Be(30);
            }
            finally
            {
                node.Stop();
            }

            
     
        }


        [TestMethod]
        public async Task FastProducerThatThrowsAnExceptionWillBeSlowedBySlowConsumerThatCatchesTheException()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                    long numberOfTimesFirstOperationWasRun = 0;

                    long numberOfTimesSecondOperationWasRun = 0;

                    async Task Method1()
                    {
                        try
                        {
                            await runner1.Run(() =>
                            {
                                SimulateWork(TimeSpan.Zero, ref numberOfTimesFirstOperationWasRun);

                                throw new Exception("Error message");
                            });
                        }
                        catch
                        {
                            await runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesSecondOperationWasRun,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);
                            });
                        }
                    }

                    var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);

                    PossibleValuesComparer
                        .AreEqual(
                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns,
                            new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                        .Should().BeTrue();
                });
            });
        }

        //TODO: add more tests for the async version
        [TestMethod]
        public async Task FastProducerWillBeSlowedBySlowConsumerForAsyncVersion()
        {
            await CreateAndUseNewAsyncBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewAsyncBlock(1, 1, async runner2 =>
                {
                    long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                    long numberOfTimesFirstOperationWasRun = 0;

                    long numberOfTimesSecondOperationWasRun = 0;

                    async Task Method1()
                    {
                        await runner1.Run(async () =>
                        {
                            Interlocked.Increment(ref numberOfTimesFirstOperationWasRun);
                        });

                        await runner2.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(100));

                            var numberOfTimes = Interlocked.Increment(ref numberOfTimesSecondOperationWasRun);

                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns[numberOfTimes - 1] = Interlocked.Read(ref numberOfTimesFirstOperationWasRun);
                        });
                    }

                    var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);

                    PossibleValuesComparer
                        .AreEqual(
                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns,
                            new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                        .Should().BeTrue();
                });
            });
        }

        [TestMethod]
        public async Task FastProducerWillBeSlowedBySlowConsumerWhenQueueSizeIs2()
        {
            await RunBasicTwoOperationsTest(
                numberOfThreadsForFirstOperation: 1,
                queue1Size: 2,
                numberOfThreadsForSecondOperation: 1,
                queye2Size: 2,
                numberOfTimesToProcess: 10,
                expectedNumberOfFirstOperationCompleteForEachTimeOperationTwoIsInvoked:
                new PossibleValues<long>[] { 4, 5, 6, 7, 8, 9, 10, 10, 10, 10 });
        }


        [TestMethod]
        public async Task FastProducerWillBeSlowedBySlowConsumerWhenQueueSizeIs2AndNumberOfThreadsIs2()
        {
            await RunBasicTwoOperationsTest(
                numberOfThreadsForFirstOperation: 2,
                queue1Size: 2,
                numberOfThreadsForSecondOperation: 2,
                queye2Size: 2,
                numberOfTimesToProcess: 10,
                expectedNumberOfFirstOperationCompleteForEachTimeOperationTwoIsInvoked:
                new PossibleValues<long>[] { 6, new[] { 6L, 7 }, 8, new[] { 8L, 9 }, 10, 10, 10, 10, 10, 10 });
        }

        [TestMethod]
        public async Task CycleInTheFlowShouldNotDeadlock()
        {
            var numberOfTimesToProcess = 10;

            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    async Task Method1()
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            await runner1.Run(() => { SimulateWork(TimeSpan.Zero); });

                            await runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100));
                            });
                        }
                    }

                    var tasks = Enumerable.Range(0, numberOfTimesToProcess).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);
                });
            });
        }

        [TestMethod]
        public async Task CycleInTheFlowWhereAnExceptionIsThrownShouldNotDeadlock()
        {
            var numberOfTimesToProcess = 10;

            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    async Task Method1()
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            try
                            {
                                await runner1.Run(() => throw new Exception("Some exception"));
                            }
                            catch {}

                            await runner2.Run(() =>
                            {
                                SimulateWork(TimeSpan.FromMilliseconds(100));
                            });
                        }
                    }

                    var tasks = Enumerable.Range(0, numberOfTimesToProcess).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);
                });
            });
        }


        [TestMethod]
        public async Task DfTaskCanBeUsedAsReturnTypeOfAsyncMethodToEnableComposition()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                    long numberOfTimesFirstOperationWasRun = 0;

                    long numberOfTimesSecondOperationWasRun = 0;

                    async DfTask Part1()
                    {
                        await runner1.Run(() =>
                        {
                            SimulateWork(TimeSpan.Zero, ref numberOfTimesFirstOperationWasRun);
                        });
                    }

                    async DfTask Part2()
                    {
                        await runner2.Run(() =>
                        {
                            SimulateWork(
                                TimeSpan.FromMilliseconds(100),
                                ref numberOfTimesSecondOperationWasRun,
                                ref numberOfTimesFirstOperationWasRun,
                                numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);
                        });
                    }

                    async Task Method1()
                    {
                        await Part1();

                        await Part2();
                    }

                    var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);

                    PossibleValuesComparer
                        .AreEqual(
                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns,
                            new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                        .Should().BeTrue();
                });
            });
        }


        [TestMethod]
        public async Task DfTaskAsAsyncReturnTypeWillFlowExecutionContext()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                async DfTask<bool> Part1(int index)
                {
                    bool result = CallContext.LogicalGetData("ContextData") as int? == index;

                    await runner1.Run(() =>
                    {
                        if (CallContext.LogicalGetData("ContextData") as int? != index)
                            result = false;

                        SimulateWork(TimeSpan.Zero);
                    });

                    if (CallContext.LogicalGetData("ContextData") as int? != index)
                        result = false;

                    return result;
                }

                async Task<bool> Method1(int index)
                {
                    bool result = CallContext.LogicalGetData("ContextData") as int? == index;

                    if (!await Part1(index))
                        result = false;

                    if (CallContext.LogicalGetData("ContextData") as int? != index)
                        result = false;

                    return result;
                }

                var tasks = Enumerable.Range(0, 10).Select(x =>
                {
                    CallContext.LogicalSetData("ContextData", x);

                    try
                    {
                        return Method1(x);
                    }
                    finally
                    {
                        CallContext.FreeNamedDataSlot("ContextData");
                    }
                }).ToArray();

                var results = await Task.WhenAll(tasks);

                results.ShouldAllBeEquivalentTo(true);

            });
        }


        [TestMethod]
        public async Task ExecutionContextFlowsCorrectly()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                async Task<bool> Method1(int index)
                {
                    bool result = true;

                    await runner1.Run(() =>
                    {
                        SimulateWork(TimeSpan.FromMilliseconds(50));

                        if (CallContext.LogicalGetData("ContextData") as int? != index)
                            result = false;
                    });

                    if (CallContext.LogicalGetData("ContextData") as int? != index)
                        result = false;

                    return result;
                }

                var tasks = Enumerable.Range(0, 10).Select(x =>
                {
                    CallContext.LogicalSetData("ContextData", x);

                    try
                    {
                        return Method1(x);
                    }
                    finally
                    {
                        CallContext.FreeNamedDataSlot("ContextData");
                    }
                }).ToArray();

                var results = await Task.WhenAll(tasks);

                results.ShouldAllBeEquivalentTo(true);
            });
        }


        [TestMethod]
        public async Task ExceptionsAreHandledCorrectlyFromTheDfTaskAwaiter()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                async Task Method1()
                {
                    await runner1.Run(() =>
                    {
                        throw new Exception("CorrectExceptionMessage");
                    });
                }

                try
                {
                    await Method1();

                    throw new Exception("No exception is thrown");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("CorrectExceptionMessage", e.Message);
                }
            });
        }

        [TestMethod]
        public async Task NoDeadlockIfExceptionIsThrownInStartOfDfTask()
        {
            async DfTask Method1()
            {
                throw new Exception("CorrectExceptionMessage");
            }

            try
            {
                await Method1();

                throw new Exception("No exception is thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("CorrectExceptionMessage", e.Message);
            }
        }

        [TestMethod]
        public async Task NoDeadlockIfMethodReturnsInStartOfDfTask()
        {
            async DfTask<int> Method1()
            {
                return 1;
            }

            (await Method1()).Should().Be(1);
        }


        [TestMethod]
        public async Task ExceptionsAreHandledCorrectlyFromTheDfTaskAsAsyncReturnValue()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                async DfTask Method1()
                {
                    await runner1.Run(() => SimulateWork(TimeSpan.Zero));

                    throw new Exception("CorrectExceptionMessage");
                }

                try
                {
                    await Method1();

                    throw new Exception("No exception is thrown");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("CorrectExceptionMessage", e.Message);
                }
            });
        }

        [TestMethod]
        public async Task MultipleStepsRunTogether()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    await CreateAndUseNewBlock(1, 1, async runner3 =>
                    {
                        long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                        long[] numberOfTimesSecondOperationWasRunWhenThirdOperationRuns = new long[10];

                        long numberOfTimesFirstOperationWasRun = 0;

                        long numberOfTimesSecondOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun = 0;

                        async Task Method1()
                        {
                            await runner1.Run(() =>
                            {
                                SimulateWork(TimeSpan.FromMilliseconds(100), ref numberOfTimesFirstOperationWasRun);
                            });

                            await runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesSecondOperationWasRun,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);
                            });

                            await runner3.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesThirdOperationWasRun,
                                    ref numberOfTimesSecondOperationWasRun,
                                    numberOfTimesSecondOperationWasRunWhenThirdOperationRuns);
                            });

                        }

                        Stopwatch sw = Stopwatch.StartNew();

                        var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                        await Task.WhenAll(tasks);

                        sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(1200), 100);
                    });
                });
            });
        }

        [TestMethod]
        public async Task DfTaskWhenAllTest()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    await CreateAndUseNewBlock(1, 1, async runner3 =>
                    {
                        long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                        long[] numberOfTimesSecondOperationWasRunWhenThirdOperationRuns = new long[10];

                        long[] numberOfTimesFirstOperationWasRunWhenThirdOperationRuns = new long[10];

                        long numberOfTimesFirstOperationWasRun = 0;

                        long numberOfTimesSecondOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun2 = 0;

                        async Task Method1()
                        {
                            var dfTask1 =  runner1.Run(() =>
                            {
                                SimulateWork(TimeSpan.FromMilliseconds(0), ref numberOfTimesFirstOperationWasRun);
                            });

                            var dfTask2 = runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(0),
                                    ref numberOfTimesSecondOperationWasRun,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);
                            });

                            await DfTask.WhenAll(dfTask1, dfTask2);

                            await runner3.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesThirdOperationWasRun,
                                    ref numberOfTimesSecondOperationWasRun,
                                    numberOfTimesSecondOperationWasRunWhenThirdOperationRuns);

                                SimulateWork(
                                    TimeSpan.FromMilliseconds(0),
                                    ref numberOfTimesThirdOperationWasRun2,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenThirdOperationRuns);
                            });

                        }
                        var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                        await Task.WhenAll(tasks);

                        PossibleValuesComparer
                            .AreEqual(
                                numberOfTimesSecondOperationWasRunWhenThirdOperationRuns,
                                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                            .Should().BeTrue();

                        PossibleValuesComparer
                            .AreEqual(
                                numberOfTimesFirstOperationWasRunWhenThirdOperationRuns,
                                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                            .Should().BeTrue();

                    });
                });
            });

        }

        [TestMethod]
        public async Task DfTaskOfTResultWhenAllTest()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    await CreateAndUseNewBlock(1, 1, async runner3 =>
                    {
                        long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                        long[] numberOfTimesSecondOperationWasRunWhenThirdOperationRuns = new long[10];

                        long[] numberOfTimesFirstOperationWasRunWhenThirdOperationRuns = new long[10];

                        long numberOfTimesFirstOperationWasRun = 0;

                        long numberOfTimesSecondOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun2 = 0;

                        async Task Method1()
                        {
                            var dfTask1 = runner1.Run(() =>
                            {
                                SimulateWork(TimeSpan.FromMilliseconds(0), ref numberOfTimesFirstOperationWasRun);

                                return 1;
                            });

                            var dfTask2 = runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(0),
                                    ref numberOfTimesSecondOperationWasRun,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);

                                return 2;
                            });

                            var results =  await DfTask.WhenAll(dfTask1, dfTask2);

                            results[0].Should().Be(1);
                            results[1].Should().Be(2);

                            await runner3.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesThirdOperationWasRun,
                                    ref numberOfTimesSecondOperationWasRun,
                                    numberOfTimesSecondOperationWasRunWhenThirdOperationRuns);

                                SimulateWork(
                                    TimeSpan.FromMilliseconds(0),
                                    ref numberOfTimesThirdOperationWasRun2,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenThirdOperationRuns);
                            });

                        }
                        var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                        await Task.WhenAll(tasks);

                        PossibleValuesComparer
                            .AreEqual(
                                numberOfTimesSecondOperationWasRunWhenThirdOperationRuns,
                                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                            .Should().BeTrue();

                        PossibleValuesComparer
                            .AreEqual(
                                numberOfTimesFirstOperationWasRunWhenThirdOperationRuns,
                                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                            .Should().BeTrue();

                    });
                });
            });

        }

        [TestMethod]
        public async Task DfTaskOfTResultWhenAllThatTakesDfTasksOfDifferentTypesTest()
        {
            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewBlock(1, 1, async runner2 =>
                {
                    await CreateAndUseNewBlock(1, 1, async runner3 =>
                    {
                        long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                        long[] numberOfTimesSecondOperationWasRunWhenThirdOperationRuns = new long[10];

                        long[] numberOfTimesFirstOperationWasRunWhenThirdOperationRuns = new long[10];

                        long numberOfTimesFirstOperationWasRun = 0;

                        long numberOfTimesSecondOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun2 = 0;

                        async Task Method1()
                        {
                            var dfTask1 = runner1.Run(() =>
                            {
                                SimulateWork(TimeSpan.FromMilliseconds(0), ref numberOfTimesFirstOperationWasRun);

                                return 1;
                            });

                            var dfTask2 = runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(0),
                                    ref numberOfTimesSecondOperationWasRun,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);

                                return "yes";
                            });

                            var results = await DfTask.WhenAll(dfTask1, dfTask2, (x,y) => x + y);

                            results.Should().Be("1yes");

                            await runner3.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesThirdOperationWasRun,
                                    ref numberOfTimesSecondOperationWasRun,
                                    numberOfTimesSecondOperationWasRunWhenThirdOperationRuns);

                                SimulateWork(
                                    TimeSpan.FromMilliseconds(0),
                                    ref numberOfTimesThirdOperationWasRun2,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenThirdOperationRuns);
                            });

                        }
                        var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                        await Task.WhenAll(tasks);

                        PossibleValuesComparer
                            .AreEqual(
                                numberOfTimesSecondOperationWasRunWhenThirdOperationRuns,
                                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                            .Should().BeTrue();

                        PossibleValuesComparer
                            .AreEqual(
                                numberOfTimesFirstOperationWasRunWhenThirdOperationRuns,
                                new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                            .Should().BeTrue();
                    });
                });
            });

        }


        [TestMethod]
        public async Task MultipleStepsRunTogether_2ThreadsPerStep()
        {
            await CreateAndUseNewBlock(2, 2, async runner1 =>
            {
                await CreateAndUseNewBlock(2, 2, async runner2 =>
                {
                    await CreateAndUseNewBlock(2, 2, async runner3 =>
                    {
                        long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                        long[] numberOfTimesSecondOperationWasRunWhenThirdOperationRuns = new long[10];

                        long numberOfTimesFirstOperationWasRun = 0;

                        long numberOfTimesSecondOperationWasRun = 0;

                        long numberOfTimesThirdOperationWasRun = 0;

                        async Task Method1()
                        {
                            await runner1.Run(() =>
                            {
                                SimulateWork(TimeSpan.FromMilliseconds(100), ref numberOfTimesFirstOperationWasRun);
                            });

                            await runner2.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesSecondOperationWasRun,
                                    ref numberOfTimesFirstOperationWasRun,
                                    numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);
                            });

                            await runner3.Run(() =>
                            {
                                SimulateWork(
                                    TimeSpan.FromMilliseconds(100),
                                    ref numberOfTimesThirdOperationWasRun,
                                    ref numberOfTimesSecondOperationWasRun,
                                    numberOfTimesSecondOperationWasRunWhenThirdOperationRuns);
                            });
                        }

                        Stopwatch sw = Stopwatch.StartNew();

                        var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                        await Task.WhenAll(tasks);

                        sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(700), 100);
                    });
                });
            });
        }

        [TestMethod]
        public async Task CancellationTokenIsRespectedInProcDataflowBlock()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            await CreateAndUseNewBlock(1, 1, async runner1 =>
            {
                long numberOfTimesFirstOperationWasRun = 0;

                async Task Method1()
                {

                    await runner1.Run(() =>
                    {
                        numberOfTimesFirstOperationWasRun++;

                        if (numberOfTimesFirstOperationWasRun == 2)
                            cts.Cancel();
                    });
                }

                var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch //WhenAll becomes faulted if any task is faulted. We expect two tasks to be faulted.
                {

                }

                Assert.AreEqual(2, numberOfTimesFirstOperationWasRun);

                Assert.IsTrue(tasks.Count(x => x.Status == TaskStatus.RanToCompletion) == 2);

                Assert.IsTrue(tasks.Count(x => x.Status == TaskStatus.Canceled) == 8);

            }, cts.Token);

        }

        [TestMethod]
        public async Task CancellationTokenIsRespectedInAsyncProcDataflowBlock()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            await CreateAndUseNewAsyncBlock(1, 1, async runner1 =>
            {
                long numberOfTimesFirstOperationWasRun = 0;

                async Task Method1()
                {
                    await runner1.Run(async () =>
                    {
                        numberOfTimesFirstOperationWasRun++;

                        if (numberOfTimesFirstOperationWasRun == 2)
                            cts.Cancel();
                    });
                }

                var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch //WhenAll becomes faulted if any task is faulted. We expect two tasks to be faulted.
                {

                }

                Assert.AreEqual(2, numberOfTimesFirstOperationWasRun);

                Assert.IsTrue(tasks.Count(x => x.Status == TaskStatus.RanToCompletion) == 2);

                Assert.IsTrue(tasks.Count(x => x.Status == TaskStatus.Canceled) == 8);

            }, cts.Token);

        }


        [TestMethod]
        public async Task DfTaskBasedMethodCanAwaitTasks()
        {
            async DfTask DoIt()
            {
                await Task.Delay(1000);
            }

            await DoIt();
        }

        //TODO: it is not supported to await a non-DfTask between two block runs. Create a Roslyn analyzer to detect this.

        [TestMethod]
        public async Task AsyncDfTaskBasedMethodCanSynchronouslyDoSomethingAndReturnValue()
        {
            async DfTask<int> DoIt()
            {
                return 5;
            }

            Assert.AreEqual(5, await DoIt());
        }

        [TestMethod]
        public async Task AsyncDfTaskBasedMethodCanSynchronouslyDoSomethingAndReturnValueTwoLevels()
        {
            async DfTask<int> DoIt()
            {
                return 5;
            }

            async DfTask<int> DoIt2()
            {
                return await DoIt();
            }

            Assert.AreEqual(5, await DoIt2());
        }

        [TestMethod]
        public async Task AsyncDfTaskBasedMethodCanSynchronouslyDoSomethingAndReturnValueThreeLevels()
        {
            async DfTask<int> DoIt()
            {
                return 5;
            }

            async DfTask<int> DoIt2()
            {
                return await DoIt();
            }

            async DfTask<int> DoIt3()
            {
                return await DoIt2();
            }

            Assert.AreEqual(5, await DoIt3());
        }

        [TestMethod]
        public async Task FastProducerWillBeSlowedBySlowConsumerForAsyncVersionThatUsesACustomThreadsBasedActionRunner()
        {
            await CreateAndUseNewAsyncBlock(1, 1, async runner1 =>
            {
                await CreateAndUseNewAsyncBlock(1, 1, async runner2 =>
                {
                    long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[10];

                    long numberOfTimesFirstOperationWasRun = 0;

                    long numberOfTimesSecondOperationWasRun = 0;

                    async Task Method1()
                    {
                        await runner1.Run(async () =>
                        {
                            Interlocked.Increment(ref numberOfTimesFirstOperationWasRun);
                        });

                        await runner2.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(100));

                            var numberOfTimes = Interlocked.Increment(ref numberOfTimesSecondOperationWasRun);

                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns[numberOfTimes - 1] = Interlocked.Read(ref numberOfTimesFirstOperationWasRun);
                        });
                    }

                    var tasks = Enumerable.Range(0, 10).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);

                    PossibleValuesComparer
                        .AreEqual(
                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns,
                            new PossibleValues<long>[] { 3, 4, 5, 6, 7, 8, 9, 10, 10, 10 })
                        .Should().BeTrue();
                }, useCustomThreadsBasedActionRunner: true);
            }, useCustomThreadsBasedActionRunner: true);
        }

        public static async Task CreateAndUseNewBlock(
            int numberOfThreads,
            int maximumNumberOfActionsInQueue,
            Func<IProcDataflowBlock, Task> action,
            CancellationToken cancellationToken = default)
        {
            var runner = new ThreadPoolBasedActionRunner();

            var node = new ProcDataflowBlock(runner, maximumNumberOfActionsInQueue, numberOfThreads, cancellationToken);

            node.Start();

            try
            {
                await action(node);
            }
            finally
            {
                node.Stop();
            }
        }

        public static async Task CreateAndUseNewAsyncBlock(
            int? maximumDegreeOfParallelism,
            int maximumNumberOfActionsInQueue,
            Func<IAsyncProcDataflowBlock, Task> action,
            CancellationToken cancellationToken = default,
            bool useCustomThreadsBasedActionRunner = false)
        {
            AsyncProcDataflowBlock node;

            CustomThreadsBasedActionRunner customThreadsBasedActionRunner = null;
            
            if (useCustomThreadsBasedActionRunner)
            {
                customThreadsBasedActionRunner = new CustomThreadsBasedActionRunner(8);

                customThreadsBasedActionRunner.Start();

                node = new AsyncProcDataflowBlock(
                    customThreadsBasedActionRunner,
                    maximumNumberOfActionsInQueue,
                    maximumDegreeOfParallelism,
                    cancellationToken);
            }
            else
            {
                node = new AsyncProcDataflowBlock(
                    maximumNumberOfActionsInQueue,
                    maximumDegreeOfParallelism,
                    cancellationToken);
            }

            node.Start();

            try
            {
                await action(node);
            }
            finally
            {
                node.Stop();

                if (useCustomThreadsBasedActionRunner)
                {
                    customThreadsBasedActionRunner.Stop();
                }
            }
        }


        private async Task RunBasicTwoOperationsTest(int numberOfThreadsForFirstOperation, int queue1Size,
            int numberOfThreadsForSecondOperation, int queye2Size, int numberOfTimesToProcess,
            PossibleValues<long>[] expectedNumberOfFirstOperationCompleteForEachTimeOperationTwoIsInvoked)
        {
            await CreateAndUseNewBlock(numberOfThreadsForFirstOperation, queue1Size, async runner1 =>
            {
                await CreateAndUseNewBlock(numberOfThreadsForSecondOperation, queye2Size, async runner2 =>
                {
                    long[] numberOfTimesFirstOperationWasRunWhenSecondOperationRuns = new long[numberOfTimesToProcess];

                    long numberOfTimesFirstOperationWasRun = 0;

                    long numberOfTimesSecondOperationWasRun = 0;

                    async Task Method1()
                    {
                        await runner1.Run(() => { SimulateWork(TimeSpan.Zero, ref numberOfTimesFirstOperationWasRun); });

                        await runner2.Run(() =>
                        {
                            SimulateWork(
                                TimeSpan.FromMilliseconds(100),
                                ref numberOfTimesSecondOperationWasRun,
                                ref numberOfTimesFirstOperationWasRun,
                                numberOfTimesFirstOperationWasRunWhenSecondOperationRuns);
                        });
                    }


                    var tasks = Enumerable.Range(0, numberOfTimesToProcess).Select(_ => Method1()).ToArray();

                    await Task.WhenAll(tasks);


                    PossibleValuesComparer
                        .AreEqual(
                            numberOfTimesFirstOperationWasRunWhenSecondOperationRuns,
                            expectedNumberOfFirstOperationCompleteForEachTimeOperationTwoIsInvoked)
                        .Should().BeTrue();
                });
            });
        }

        private static void SimulateWork(
            TimeSpan timeToWork,
            ref long numberOfTimesInvoked,
            ref long numberOfTimesOtherOperationInvoked,
            long[] arrayOfNumberOfTimesOtherOperationInvokedForEachTimeThisOperationIsInvoked)
        {
            var numberOfTimes = SimulateWork(timeToWork, ref numberOfTimesInvoked);

            arrayOfNumberOfTimesOtherOperationInvokedForEachTimeThisOperationIsInvoked[numberOfTimes - 1] = Interlocked.Read(ref numberOfTimesOtherOperationInvoked);
        }

        private static long SimulateWork(TimeSpan timeToWork, ref long numberOfTimesInvoked)
        {
            SimulateWork(timeToWork);

            return Interlocked.Increment(ref numberOfTimesInvoked);
        }

        private static void SimulateWork(TimeSpan timeToWork)
        {
            if (timeToWork > TimeSpan.Zero)
                Thread.Sleep(timeToWork);
        }

    }
}