using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests
{
    [TestClass]
    public class AsyncRelayCommandTests
    {
        [TestMethod]
        public void TestConstructorNeedsExecute()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new AsyncRelayCommand(null)
            );
        }

        [TestMethod]
        public void TestConstructorDoesNotNeedCanExecute()
        {
            var cmd = new AsyncRelayCommand(
                () => Task.CompletedTask,
                null
            );
        }


        [TestMethod]
        public void TestCanExecuteRunsDelegate()
        {
            int canExecuteRunCount = 0;
            Func<bool> canExecute = () =>
            {
                canExecuteRunCount++;
                return true;
            };

            var cmd = new AsyncRelayCommand(() => Task.CompletedTask, canExecute);

            cmd.CanExecute();

            Assert.AreEqual(1, canExecuteRunCount);
        }

        [TestMethod]
        public void TestCanExecuteYieldsTrueWithoutDelegate()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask, null);

            bool canExecute = cmd.CanExecute();

            Assert.AreEqual(true, canExecute);
        }

        [TestMethod]
        public void TestExplicitCanExecute()
        {
            int canExecuteRunCount = 0;
            Func<bool> canExecute = () =>
            {
                canExecuteRunCount++;
                return true;
            };

            ICommand cmd = new AsyncRelayCommand(() => Task.CompletedTask, canExecute);

            cmd.CanExecute(null);

            Assert.AreEqual(1, canExecuteRunCount);
        }


        [TestMethod]
        public void TestExecuteRunsDelegate()
        {
            int executeRunCount = 0;

            // note: this needs to be 100% synchronous so we don't end up with a race condition
            Func<Task> execute = () =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            var cmd = new AsyncRelayCommand(execute);

            cmd.Execute();

            Assert.AreEqual(1, executeRunCount);
        }

        [TestMethod]
        public void TestExecutionSetOnExecute()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask);

            cmd.Execute();

            Assert.IsNotNull(cmd.Execution);
        }

        [TestMethod]
        public void TestPropertyChangedRaisedOnExecuteForExecution()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask);

            var changes = new List<string>();
            cmd.PropertyChanged += (sender, e) =>
            {
                changes.Add(e.PropertyName);
            };

            cmd.Execute();

            CollectionAssert.AreEqual(new[] { nameof(AsyncRelayCommand.Execution) }, changes);
        }

        [TestMethod]
        public void TestExplicitExecute()
        {
            int executeRunCount = 0;

            Func<Task> execute = () =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            ICommand cmd = new AsyncRelayCommand(execute);

            cmd.Execute(null);

            Assert.AreEqual(1, executeRunCount);
        }


        [TestMethod]
        public async Task TestExecuteAsyncRunsDelegate()
        {
            int executeRunCount = 0;

            Func<Task> execute = () =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            var cmd = new AsyncRelayCommand(execute);

            await cmd.ExecuteAsync();

            Assert.AreEqual(1, executeRunCount);
        }

        [TestMethod]
        public async Task TestExecutionSetOnExecuteAsync()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask);

            var t = cmd.ExecuteAsync();

            Assert.IsNotNull(cmd.Execution);

            await t;
        }

        [TestMethod]
        public async Task TestPropertyChangedRaisedOnExecuteAsyncForExecution()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask);

            var changes = new List<string>();
            cmd.PropertyChanged += (sender, e) =>
            {
                changes.Add(e.PropertyName);
            };

            var t = cmd.ExecuteAsync();

            CollectionAssert.AreEqual(new[] { nameof(AsyncRelayCommand.Execution) }, changes);

            await t;
        }

        [TestMethod]
        public async Task TestExplicitExecuteAsyncRunsDelegate()
        {
            int executeRunCount = 0;

            Func<Task> execute = () =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            IAsyncCommand cmd = new AsyncRelayCommand(execute);

            await cmd.ExecuteAsync(null);

            Assert.AreEqual(1, executeRunCount);
        }


        [TestMethod]
        public void TestRaiseCanExecuteChanged()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask);

            int eventFiredCount = 0;
            cmd.CanExecuteChanged += (sender, e) =>
            {
                eventFiredCount++;
            };

            cmd.RaiseCanExecuteChanged();

            Assert.AreEqual(1, eventFiredCount);
        }

        [TestMethod]
        public void TestRaiseCanExecuteChangedWithoutHandler()
        {
            var cmd = new AsyncRelayCommand(() => Task.CompletedTask);

            // "assert" that this does not throw
            cmd.RaiseCanExecuteChanged();
        }
    }
}
