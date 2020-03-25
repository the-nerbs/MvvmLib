using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests.Standalone
{
    [TestClass]
    public class RelayCommandTTests
    {
        [TestMethod]
        public void TestCtorNeedsExecute()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new RelayCommand<object>(null)
            );

            Assert.ThrowsException<ArgumentNullException>(
                () => new RelayCommand<object>(null, (x) => true)
            );
        }

        [TestMethod]
        public void TestCtorDoesNotNeedCanExecute()
        {
            var cmd = new RelayCommand<object>((x) => { }, null);

            Assert.IsTrue(cmd.CanExecute(null));
        }

        [TestMethod]
        public void TestExecuteInvokesDelegate()
        {
            int callCount = 0;
            Action<object> execute = x => callCount++;
            var cmd = new RelayCommand<object>(execute);

            cmd.Execute(null);

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void TestICommandExecuteInvokesDelegate()
        {
            int callCount = 0;
            Action<object> execute = x => callCount++;
            var cmd = new RelayCommand<object>(execute);

            cmd.Execute(null);

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(1)]
        [DataRow("some string")]
        public void TestICommandExecuteAllowsCompatibleParameters(object param)
        {
            int callCount = 0;
            Action<object> execute = x => callCount++;
            var cmd = new RelayCommand<object>(execute);

            ICommand icmd = cmd;
            icmd.Execute(param);

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void TestICommandExecuteThrowsOnIncompatibleParameters()
        {
            int callCount = 0;
            Action<int> execute = x => callCount++;
            var cmd = new RelayCommand<int>(execute);

            ICommand icmd = cmd;

            Assert.ThrowsException<InvalidCastException>(
                () => icmd.Execute("not an int")
            );

            Assert.AreEqual(0, callCount);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void TestCanExecuteInvokesDelegate(bool expectedResult)
        {
            int callCount = 0;
            Func<object, bool> canExecute = x =>
            {
                callCount++;
                return expectedResult;
            };

            var cmd = new RelayCommand<object>(x => { }, canExecute);

            bool result = cmd.CanExecute(null);

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
        public void TestICommandCanExecuteAllowsCompatibleParameters(object param, bool expectedResult)
        {
            int callCount = 0;
            Func<object, bool> canExecute = x =>
            {
                callCount++;
                return expectedResult;
            };

            var cmd = new RelayCommand<object>(x => { }, canExecute);

            ICommand icmd = cmd;
            bool result = icmd.CanExecute(param);

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void TestICommandCanExecuteThrowsOnIncompatibleParameters()
        {
            int callCount = 0;
            Func<int, bool> canExecute = x =>
            {
                callCount++;
                return true;
            };

            var cmd = new RelayCommand<int>(x => { }, canExecute);

            ICommand icmd = cmd;

            Assert.ThrowsException<InvalidCastException>(
                () => icmd.CanExecute("not an int")
            );

            Assert.AreEqual(0, callCount);
        }


        [TestMethod]
        public void TestRaiseCanExecuteChanged()
        {
            var calls = new List<(object sender, EventArgs e)>();
            EventHandler handler = (sender, e) =>
            {
                calls.Add((sender, e));
            };

            var cmd = new RelayCommand<object>(x => { });
            cmd.CanExecuteChanged += handler;

            cmd.RaiseCanExecuteChanged();

            Assert.AreEqual(1, calls.Count);
            Assert.AreSame(cmd, calls[0].sender);
            Assert.AreSame(EventArgs.Empty, calls[0].e);
        }

        [TestMethod]
        public void TestRaiseCanExecuteChangedDoesNotFailIfNoHandlers()
        {
            var cmd = new RelayCommand<object>(x => { });

            // assert that this does not throw
            cmd.RaiseCanExecuteChanged();
        }
    }
}
