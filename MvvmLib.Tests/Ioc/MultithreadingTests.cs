using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    [TestClass]
    public class MultithreadingTests
    {
        [TestMethod]
        public async Task TestAsyncRead()
        {
            bool done = false;
            var ioc = new IocContainer();
            ioc.Bind<ITest, Test>();

            Task t;

            try
            {
                bool started = false;

                // there shouldn't be any issues from reading from multiple contexts.
                t = Task.Run(() =>
                {
                    Volatile.Write(ref started, true);

                    while (!Volatile.Read(ref done))
                    {
                        ITest instance = ioc.Resolve<ITest>();
                        Assert.IsNotNull(instance);
                    }
                });

                while (!Volatile.Read(ref started))
                {
                    await Task.Delay(10);
                }

                for (int i = 0; i < 1000; i++)
                {
                    ITest instance = ioc.Resolve<ITest>();
                    Assert.IsNotNull(instance);
                }
            }
            finally
            {
                done = true;
            }

            await t;
        }

        [TestMethod]
        public async Task TestAsyncWrite()
        {
            bool done = false;
            var ioc = new IocContainer();
            ioc.Bind<ITest, Test>();

            Task t;

            try
            {
                bool started = false;

                // The Bind calls from the task and here could corrupt the
                // container. Make sure it doesn't
                t = Task.Run(() =>
                {
                    Volatile.Write(ref started, true);

                    while (!Volatile.Read(ref done))
                    {
                        ioc.Bind<ITest, Test>();
                    }
                });

                while (!Volatile.Read(ref started))
                {
                    await Task.Delay(10);
                }

                for (int i = 0; i < 1000; i++)
                {
                    ioc.Bind<ITest, Test>();
                }

                ITest instance = ioc.Resolve<ITest>();
                Assert.IsNotNull(instance);
            }
            finally
            {
                done = true;
            }

            await t;
        }

        [TestMethod]
        public async Task TestAsyncReadAndWrite()
        {
            bool done = false;
            var ioc = new IocContainer();
            ioc.Bind<ITest, Test>();

            Task t;

            try
            {
                bool started = false;

                // The Bind calls from the task and here could corrupt the
                // container. Make sure it doesn't
                t = Task.Run(() =>
                {
                    Volatile.Write(ref started, true);

                    while (!Volatile.Read(ref done))
                    {
                        ioc.Bind<ITest, Test>();
                    }
                });

                while (!Volatile.Read(ref started))
                {
                    await Task.Delay(10);
                }

                for (int i = 0; i < 1000; i++)
                {
                    ITest instance = ioc.Resolve<ITest>();
                    Assert.IsNotNull(instance);
                }

            }
            finally
            {
                done = true;
            }

            await t;
        }


        interface ITest
        { }

        class Test : ITest
        { }
    }
}
