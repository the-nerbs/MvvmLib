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
    public class AsyncRelayCommandTTests
    {
        [TestMethod]
        public void TestConstructorNeedsExecute()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new AsyncRelayCommand<int>(null)
            );
        }

        [TestMethod]
        public void TestConstructorDoesNotNeedCanExecute()
        {
            var cmd = new AsyncRelayCommand<int>(
                (x) => Task.CompletedTask,
                null
            );
        }


        [TestMethod]
        public void TestCanExecuteRunsDelegate()
        {
            int canExecuteRunCount = 0;
            Func<int, bool> canExecute = (x) =>
            {
                canExecuteRunCount++;
                return true;
            };

            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask, canExecute);

            cmd.CanExecute(5);

            Assert.AreEqual(1, canExecuteRunCount);
        }

        [TestMethod]
        public void TestCanExecuteYieldsTrueWithoutDelegate()
        {
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask, null);

            bool canExecute = cmd.CanExecute(5);

            Assert.AreEqual(true, canExecute);
        }

        [TestMethod]
        public void TestExplicitCanExecute()
        {
            int canExecuteRunCount = 0;
            Func<int, bool> canExecute = (x) =>
            {
                canExecuteRunCount++;
                return true;
            };

            ICommand cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask, canExecute);

            cmd.CanExecute(5);

            Assert.AreEqual(1, canExecuteRunCount);
        }

        [TestMethod]
        public void TestExplicitCanExecuteWrongType()
        {
            int canExecuteRunCount = 0;
            Func<int, bool> canExecute = (x) =>
            {
                canExecuteRunCount++;
                return true;
            };

            ICommand cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask, canExecute);

            Assert.ThrowsException<InvalidCastException>(
                () => cmd.CanExecute("")
            );

            Assert.AreEqual(0, canExecuteRunCount);
        }


        [TestMethod]
        public void TestExecuteRunsDelegate()
        {
            int executeRunCount = 0;

            // note: this needs to be 100% synchronous so we don't end up with a race condition
            Func<int, Task> execute = (x) =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            var cmd = new AsyncRelayCommand<int>(execute);

            cmd.Execute(5);

            Assert.AreEqual(1, executeRunCount);
        }

        [TestMethod]
        public void TestExecutionSetOnExecute()
        {
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask);

            cmd.Execute(5);

            Assert.IsNotNull(cmd.Execution);
        }

        [TestMethod]
        public void TestPropertyChangedRaisedOnExecuteForExecution()
        {
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask);

            var changes = new List<string>();
            cmd.PropertyChanged += (sender, e) =>
            {
                changes.Add(e.PropertyName);
            };

            cmd.Execute(5);

            CollectionAssert.AreEqual(new[] { nameof(AsyncRelayCommand.Execution) }, changes);
        }

        [TestMethod]
        public void TestExplicitExecute()
        {
            int executeRunCount = 0;

            Func<int, Task> execute = (x) =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            ICommand cmd = new AsyncRelayCommand<int>(execute);

            cmd.Execute(5);

            Assert.AreEqual(1, executeRunCount);
        }

        [TestMethod]
        public void TestExplicitExecuteWrongType()
        {
            int executeRunCount = 0;

            Func<int, Task> execute = (x) =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            ICommand cmd = new AsyncRelayCommand<int>(execute);

            Assert.ThrowsException<InvalidCastException>(
                () => cmd.Execute("")
            );

            Assert.AreEqual(0, executeRunCount);
        }


        [TestMethod]
        public async Task TestExecuteAsyncRunsDelegate()
        {
            int executeRunCount = 0;

            Func<int, Task> execute = (x) =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            var cmd = new AsyncRelayCommand<int>(execute);

            await cmd.ExecuteAsync(5);

            Assert.AreEqual(1, executeRunCount);
        }

        [TestMethod]
        public async Task TestExecutionSetOnExecuteAsync()
        {
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask);

            var t = cmd.ExecuteAsync(5);

            Assert.IsNotNull(cmd.Execution);

            await t;
        }

        [TestMethod]
        public async Task TestPropertyChangedRaisedOnExecuteAsyncForExecution()
        {
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask);

            var changes = new List<string>();
            cmd.PropertyChanged += (sender, e) =>
            {
                changes.Add(e.PropertyName);
            };

            var t = cmd.ExecuteAsync(5);

            CollectionAssert.AreEqual(new[] { nameof(AsyncRelayCommand.Execution) }, changes);

            await t;
        }

        [TestMethod]
        public async Task TestExplicitExecuteAsync()
        {
            int executeRunCount = 0;

            Func<int, Task> execute = (x) =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            IAsyncCommand cmd = new AsyncRelayCommand<int>(execute);

            await cmd.ExecuteAsync(5);

            Assert.AreEqual(1, executeRunCount);
        }

        [TestMethod]
        public async Task TestExplicitExecuteAsyncWrongType()
        {
            int executeRunCount = 0;

            Func<int, Task> execute = (x) =>
            {
                executeRunCount++;
                return Task.CompletedTask;
            };

            IAsyncCommand cmd = new AsyncRelayCommand<int>(execute);

            
            await Assert.ThrowsExceptionAsync<InvalidCastException>(
                async () => await cmd.ExecuteAsync("")
            );

            Assert.AreEqual(0, executeRunCount);
        }


        [TestMethod]
        public void TestRaiseCanExecuteChanged()
        {
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask);

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
            var cmd = new AsyncRelayCommand<int>((x) => Task.CompletedTask);

            // "assert" that this does not throw
            cmd.RaiseCanExecuteChanged();
        }
    }
}
