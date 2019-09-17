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
    public class TaskExecutionTests
    {
        [TestMethod]
        public void TestConstructorRequiresTask()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TaskExecution(null)
            );
        }

        [TestMethod]
        public void TestWatchedTaskMayAlreadyBeCompleted()
        {
            Task t = Task.CompletedTask;

            var execution = new TaskExecution(t);

            Assert.AreSame(t, execution.Task);
            Assert.IsTrue(execution.IsCompleted);
            Assert.IsTrue(execution.CompletionTask.IsCompleted);
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
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution(task);

                    Assert.AreSame(task, e.Task);
                    Assert.AreNotSame(task, e.CompletionTask);

                    Assert.AreEqual(TaskStatus.Running, e.Status);

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
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution(task);

                    Volatile.Write(ref complete, true);
                    await e.CompletionTask;

                    Assert.AreSame(task, e.Task);
                    Assert.AreNotSame(task, e.CompletionTask);
                    Assert.AreEqual(TaskStatus.RanToCompletion, e.Status);
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
            var task = Task.FromCanceled(token);
            var e = new TaskExecution(task);

            Assert.AreSame(task, e.Task);
            Assert.AreNotSame(task, e.CompletionTask);
            Assert.AreEqual(TaskStatus.Canceled, e.Status);
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
            var task = Task.FromException(exception);
            var e = new TaskExecution(task);

            Assert.AreSame(task, e.Task);
            Assert.AreNotSame(task, e.CompletionTask);
            Assert.AreEqual(TaskStatus.Faulted, e.Status);
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
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution(task);

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
                            nameof(TaskExecution.Status),
                            nameof(TaskExecution.IsCompleted),
                            nameof(TaskExecution.IsCompletedSuccessfully),
                            nameof(TaskExecution.IsNotCompleted),
                            nameof(TaskExecution.IsCanceled),
                            nameof(TaskExecution.IsFaulted),
                            nameof(TaskExecution.Exception),
                            nameof(TaskExecution.InnerException),
                            nameof(TaskExecution.InnerExceptions),
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
                    }, token);

                    try
                    {
                        evnt.Wait();
                        var e = new TaskExecution(task);

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
                                nameof(TaskExecution.Status),
                                nameof(TaskExecution.IsCompleted),
                                nameof(TaskExecution.IsCompletedSuccessfully),
                                nameof(TaskExecution.IsNotCompleted),
                                nameof(TaskExecution.IsCanceled),
                                nameof(TaskExecution.IsFaulted),
                                nameof(TaskExecution.Exception),
                                nameof(TaskExecution.InnerException),
                                nameof(TaskExecution.InnerExceptions),
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

                    throw new Exception("test");
                });

                try
                {
                    evnt.Wait();
                    var e = new TaskExecution(task);

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
                            nameof(TaskExecution.Status),
                            nameof(TaskExecution.IsCompleted),
                            nameof(TaskExecution.IsCompletedSuccessfully),
                            nameof(TaskExecution.IsNotCompleted),
                            nameof(TaskExecution.IsCanceled),
                            nameof(TaskExecution.IsFaulted),
                            nameof(TaskExecution.Exception),
                            nameof(TaskExecution.InnerException),
                            nameof(TaskExecution.InnerExceptions),
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
