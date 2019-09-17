using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests
{
    [TestClass]
    public class TaskExecutionTTests
    {
        [TestMethod]
        public void TestConstructorRequiresTask()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TaskExecution<int>(null)
            );
        }

        [TestMethod]
        public void TestWatchedTaskMayAlreadyBeCompleted()
        {
            var task = Task.FromResult(5);

            var e = new TaskExecution<int>(task);

            Assert.AreSame(task, e.Task);
            Assert.IsTrue(e.IsCompleted);
            Assert.IsTrue(e.CompletionTask.IsCompleted);
            Assert.AreEqual(5, e.Result);
        }

        [TestMethod]
        public void TestTaskRunningState()
        {
            bool complete = false;
            using (var evnt = new ManualResetEventSlim())
            {
                var task = Task.Run(() =>
                {
                    evnt.Set();
                    int counter = int.MinValue;
                    while (!Volatile.Read(ref complete))
                    {
                        unchecked
                        {
                            counter++;
                        }
                    }
                    return 5;
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution<int>(task, defaultResult:-1);

                    Assert.AreSame(task, e.Task);
                    Assert.AreNotSame(task, e.CompletionTask);
                    Assert.AreEqual(TaskStatus.Running, e.Status);
                    Assert.AreEqual(-1, e.Result);
                    Assert.IsFalse(e.IsCompleted);
                    Assert.IsFalse(e.IsCompletedSuccessfully);
                    Assert.IsTrue(e.IsNotCompleted);
                    Assert.IsFalse(e.IsCanceled);
                    Assert.IsFalse(e.IsFaulted);
                    Assert.IsNull(e.Exception);
                    Assert.IsNull(e.InnerException);
                    Assert.IsNull(e.InnerExceptions);
                }
                finally
                {
                    Volatile.Write(ref complete, true);
                }
            }
        }

        [TestMethod]
        public async Task TestTaskCompletedSuccessfullyState()
        {
            bool complete = false;
            using (var evnt = new ManualResetEventSlim())
            {
                var task = Task.Run(() =>
                {
                    evnt.Set();
                    int counter = int.MinValue;
                    while (!Volatile.Read(ref complete))
                    {
                        unchecked
                        {
                            counter++;
                        }
                    }
                    return 5;
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution<int>(task, defaultResult: -1);

                    Volatile.Write(ref complete, true);
                    await e.CompletionTask;

                    Assert.AreSame(task, e.Task);
                    Assert.AreNotSame(task, e.CompletionTask);
                    Assert.AreEqual(TaskStatus.RanToCompletion, e.Status);
                    Assert.AreEqual(5, e.Result);
                    Assert.IsTrue(e.IsCompleted);
                    Assert.IsTrue(e.IsCompletedSuccessfully);
                    Assert.IsFalse(e.IsNotCompleted);
                    Assert.IsFalse(e.IsCanceled);
                    Assert.IsFalse(e.IsFaulted);
                    Assert.IsNull(e.Exception);
                    Assert.IsNull(e.InnerException);
                    Assert.IsNull(e.InnerExceptions);
                }
                finally
                {
                    complete = true;
                }
            }
        }

        [TestMethod]
        public void TestTaskCanceledState()
        {
            var token = new CancellationToken(true);
            var task = Task.FromCanceled<int>(token);
            var e = new TaskExecution<int>(task, defaultResult: -1);

            Assert.AreSame(task, e.Task);
            Assert.AreNotSame(task, e.CompletionTask);
            Assert.AreEqual(TaskStatus.Canceled, e.Status);
            Assert.AreEqual(-1, e.Result);
            Assert.IsTrue(e.IsCompleted);
            Assert.IsFalse(e.IsCompletedSuccessfully);
            Assert.IsFalse(e.IsNotCompleted);
            Assert.IsTrue(e.IsCanceled);
            Assert.IsFalse(e.IsFaulted);
            Assert.IsNull(e.Exception);
            Assert.IsNull(e.InnerException);
            Assert.IsNull(e.InnerExceptions);
        }

        [TestMethod]
        public void TestTaskFaultedState()
        {
            var exception = new Exception("test");
            var task = Task.FromException<int>(exception);
            var e = new TaskExecution<int>(task, defaultResult: -1);

            Assert.AreSame(task, e.Task);
            Assert.AreNotSame(task, e.CompletionTask);
            Assert.AreEqual(TaskStatus.Faulted, e.Status);
            Assert.AreEqual(-1, e.Result);
            Assert.IsTrue(e.IsCompleted);
            Assert.IsFalse(e.IsCompletedSuccessfully);
            Assert.IsFalse(e.IsNotCompleted);
            Assert.IsFalse(e.IsCanceled);
            Assert.IsTrue(e.IsFaulted);
            Assert.IsNotNull(e.Exception);
            Assert.AreSame(exception, e.InnerException);
            CollectionAssert.AreEqual(new[] { exception }, e.InnerExceptions.ToArray());
        }


        [TestMethod]
        public async Task TestPropertyChangesWhenTaskCompletesSuccessfully()
        {
            bool complete = false;
            using (var evnt = new ManualResetEventSlim())
            {
                var task = Task.Run(() =>
                {
                    evnt.Set();
                    int counter = int.MinValue;
                    while (!Volatile.Read(ref complete))
                    {
                        unchecked
                        {
                            counter++;
                        }
                    }
                    return 5;
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution<int>(task);

                    var changes = new List<string>();
                    e.PropertyChanged += (sender, args) =>
                    {
                        changes.Add(args.PropertyName);
                    };

                    Volatile.Write(ref complete, true);

                    await e.CompletionTask;

                    CollectionAssert.AreEquivalent(
                        new string[]
                        {
                            nameof(TaskExecution<int>.Status),
                            nameof(TaskExecution<int>.Result),
                            nameof(TaskExecution<int>.IsCompleted),
                            nameof(TaskExecution<int>.IsCompletedSuccessfully),
                            nameof(TaskExecution<int>.IsNotCompleted),
                            nameof(TaskExecution<int>.IsCanceled),
                            nameof(TaskExecution<int>.IsFaulted),
                            nameof(TaskExecution<int>.Exception),
                            nameof(TaskExecution<int>.InnerException),
                            nameof(TaskExecution<int>.InnerExceptions),
                        }, changes);
                }
                finally
                {
                    complete = true;
                }
            }
        }

        [TestMethod]
        public async Task TestPropertyChangesWhenTaskCancelled()
        {
            bool complete = false;

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                using (var evnt = new ManualResetEventSlim())
                {
                    var task = Task.Run(() =>
                    {
                        evnt.Set();
                        int counter = int.MinValue;
                        while (!Volatile.Read(ref complete))
                        {
                            unchecked
                            {
                                counter++;
                            }
                        }

                        cts.Cancel();
                        token.ThrowIfCancellationRequested();

                        return 5;
                    }, token);

                    try
                    {
                        evnt.Wait();
                        var e = new TaskExecution<int>(task);

                        var changes = new List<string>();
                        e.PropertyChanged += (sender, args) =>
                        {
                            changes.Add(args.PropertyName);
                        };

                        Volatile.Write(ref complete, true);

                        await e.CompletionTask;

                        CollectionAssert.AreEquivalent(
                            new string[]
                            {
                                nameof(TaskExecution<int>.Status),
                                nameof(TaskExecution<int>.Result),
                                nameof(TaskExecution<int>.IsCompleted),
                                nameof(TaskExecution<int>.IsCompletedSuccessfully),
                                nameof(TaskExecution<int>.IsNotCompleted),
                                nameof(TaskExecution<int>.IsCanceled),
                                nameof(TaskExecution<int>.IsFaulted),
                                nameof(TaskExecution<int>.Exception),
                                nameof(TaskExecution<int>.InnerException),
                                nameof(TaskExecution<int>.InnerExceptions),
                            }, changes);
                    }
                    finally
                    {
                        complete = true;
                    }
                }
            }
        }

        [TestMethod]
        public async Task TestPropertyChangesWhenTaskFaulted()
        {
            bool complete = false;

            using (var evnt = new ManualResetEventSlim())
            {
                var task = Task.Run(new Func<int>(() =>
                {
                    evnt.Set();
                    int counter = int.MinValue;
                    while (!Volatile.Read(ref complete))
                    {
                        unchecked
                        {
                            counter++;
                        }
                    }

                    throw new Exception("test");
                }));

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution<int>(task);

                    var changes = new List<string>();
                    e.PropertyChanged += (sender, args) =>
                    {
                        changes.Add(args.PropertyName);
                    };

                    Volatile.Write(ref complete, true);

                    await e.CompletionTask;

                    CollectionAssert.AreEquivalent(
                        new string[]
                        {
                            nameof(TaskExecution<int>.Status),
                            nameof(TaskExecution<int>.Result),
                            nameof(TaskExecution<int>.IsCompleted),
                            nameof(TaskExecution<int>.IsCompletedSuccessfully),
                            nameof(TaskExecution<int>.IsNotCompleted),
                            nameof(TaskExecution<int>.IsCanceled),
                            nameof(TaskExecution<int>.IsFaulted),
                            nameof(TaskExecution<int>.Exception),
                            nameof(TaskExecution<int>.InnerException),
                            nameof(TaskExecution<int>.InnerExceptions),
                        }, changes);
                }
                finally
                {
                    complete = true;
                }
            }
        }
    }
}
