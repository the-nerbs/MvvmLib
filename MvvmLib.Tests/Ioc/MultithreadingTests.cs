using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonServiceLocator;
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

        [TestMethod]
        public async Task TestAsyncSingleInstanceActivation()
        {
            var ioc = new IocContainer();

            bool started = false;

            // there shouldn't be any issues from reading from multiple contexts.
            Task<ITest> t = Task.Run(() =>
            {
                Volatile.Write(ref started, true);

                ioc.Bind<ITest>(
                    factory: Test.Create,
                    singleInstance: true
                );

                return ioc.Resolve<ITest>();
            });

            while (!Volatile.Read(ref started))
            {
                await Task.Delay(10);
            }

            ITest instance = ioc.Resolve<ITest>();
            Assert.IsNotNull(instance);

            ITest fromTask = await t;
            Assert.AreSame(instance, fromTask);
        }

        [TestMethod]
        [TestCategory("SkipWhenLiveUnitTesting")] // tests a 5 second timeout
        public async Task TestAsyncSingleInstanceActivationTimeout()
        {
            var ioc = new IocContainer();

            ioc.Bind<ITest>(
                factory: Test.Timeout,
                singleInstance: true
            );

            bool started = false;

            // there shouldn't be any issues from reading from multiple contexts.
            Task<Exception> t = Task.Run(() =>
            {
                Volatile.Write(ref started, true);

                try
                {
                    ioc.Resolve<ITest>();
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            });

            while (!Volatile.Read(ref started))
            {
                await Task.Delay(10);
            }


            Exception fromHere;
            try
            {
                ioc.Resolve<ITest>();
                fromHere = null;
            }
            catch (Exception ex2)
            {
                fromHere = ex2;
            }

            Exception fromTask = await t;

            // one should be null, and the other should be non-null
            Assert.IsTrue((fromHere is null) != (fromTask is null));

            // only one should be null
            Exception ex = fromHere ?? fromTask;
            Assert.IsInstanceOfType(ex, typeof(ActivationException));
        }

        [TestMethod]
        public async Task TestAsyncSingleInstanceActivationFailure()
        {
            var ioc = new IocContainer();

            ioc.Bind<ITest>(
                factory: Test.Failure,
                singleInstance: true
            );

            bool started = false;

            // there shouldn't be any issues from reading from multiple contexts.
            var t = Task.Run(() =>
            {
                Volatile.Write(ref started, true);

                return Assert.ThrowsException<ActivationException>(
                    () => ioc.Resolve<ITest>(),
                    "Activation should have thrown ActivationException in task."
                );
            });

            while (!Volatile.Read(ref started))
            {
                await Task.Delay(10);
            }


            var fromHere = Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ITest>(),
                "Activation should have thrown ActivationException outside of task."
            );
            var fromTask = await t;

            // the inner exception should be the same
            Assert.AreSame(fromHere.InnerException, fromTask.InnerException);
        }


        interface ITest
        { }

        class Test : ITest
        {
            public static Test Create()
            {
                Thread.Sleep(100);

                return new Test();
            }

            public static Test Timeout()
            {
                // Timeout in Registration+LazyLoader is 5 seconds (= 5000ms)
                Thread.Sleep(5100);

                return new Test();
            }

            public static Test Failure()
            {
                throw new Exception("test");
            }
        }
    }
}
