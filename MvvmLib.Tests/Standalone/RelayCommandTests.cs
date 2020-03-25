using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests.Standalone
{
    [TestClass]
    public class RelayCommandTests
    {
        [TestMethod]
        public void TestCtorNeedsExecute()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new RelayCommand(null)
            );

            Assert.ThrowsException<ArgumentNullException>(
                () => new RelayCommand(null, () => true)
            );
        }

        [TestMethod]
        public void TestCtorDoesNotNeedCanExecute()
        {
            var cmd = new RelayCommand(() => { }, null);

            Assert.IsTrue(cmd.CanExecute());
        }

        [TestMethod]
        public void TestExecuteInvokesDelegate()
        {
            int callCount = 0;
            Action execute = () => callCount++;
            var cmd = new RelayCommand(execute);

            cmd.Execute();

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void TestICommandExecuteInvokesDelegate()
        {
            int callCount = 0;
            Action execute = () => callCount++;
            var cmd = new RelayCommand(execute);

            cmd.Execute();

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(1)]
        [DataRow("some string")]
        public void TestICommandExecuteIgnoresParameter(object param)
        {
            int callCount = 0;
            Action execute = () => callCount++;
            var cmd = new RelayCommand(execute);

            ICommand icmd = cmd;
            icmd.Execute(param);

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TestCanExecuteInvokesDelegate(bool expectedResult)
        {
            int callCount = 0;
            Func<bool> canExecute = () =>
            {
                callCount++;
                return expectedResult;
            };

            var cmd = new RelayCommand(() => { }, canExecute);

            bool result = cmd.CanExecute();

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        [DataRow(null, true)]
        [DataRow(1, true)]
        [DataRow("some string", true)]
        [DataRow(null, false)]
        [DataRow(1, false)]
        [DataRow("some string", false)]
        public void TestICommandCanExecuteIgnoresParameter(object param, bool expectedResult)
        {
            int callCount = 0;
            Func<bool> canExecute = () =>
            {
                callCount++;
                return expectedResult;
            };

            var cmd = new RelayCommand(() => { }, canExecute);

            ICommand icmd = cmd;
            bool result = icmd.CanExecute(param);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(1, callCount);
        }


        [TestMethod]
        public void TestRaiseCanExecuteChanged()
        {
            var calls = new List<(object sender, EventArgs e)>();
            EventHandler handler = (sender, e) =>
            {
                calls.Add((sender, e));
            };

            var cmd = new RelayCommand(() => { });
            cmd.CanExecuteChanged += handler;

            cmd.RaiseCanExecuteChanged();

            Assert.AreEqual(1, calls.Count);
            Assert.AreSame(cmd, calls[0].sender);
            Assert.AreSame(EventArgs.Empty, calls[0].e);
        }

        [TestMethod]
        public void TestRaiseCanExecuteChangedDoesNotFailIfNoHandlers()
        {
            var cmd = new RelayCommand(() => { });

            // assert that this does not throw
            cmd.RaiseCanExecuteChanged();
        }
    }
}
