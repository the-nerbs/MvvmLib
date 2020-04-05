using System;
using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    [TestClass]
    public class RecursionTests
    {
        // note: we can't have the `WhenEnabled` counterpart for this one since it results in a
        // StackOverflowException, which tears down the process. This happens even if
        // System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute is
        // used, which is ignored in .NET (Core) anyway.
        [TestMethod]
        public void TestRecursiveResolutionWhenDisabledThrowsForCyclicDependencies()
        {
            var ioc = new IocContainer(allowRecursiveResolution: false);
            ioc.Bind<IServiceLocator>(() => ioc);
            ioc.Bind<IExampleA, ExampleA>();
            ioc.Bind<IExampleB, ExampleB>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<IExampleA>()
            );
        }

        [TestMethod]
        public void TestRecursiveResolutionWhenDisabledNotOkForAcyclicDependencies()
        {
            var ioc = new IocContainer(allowRecursiveResolution: false);
            ioc.Bind<IServiceLocator>(() => ioc);
            ioc.Bind<IExampleA, RecursiveNonCyclicA>();
            ioc.Bind<IExampleB, RecursiveNonCyclicB>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<IExampleB>()
            );
        }

        [TestMethod]
        public void TestRecursiveResolutionWhenEnabledOkForAcyclicDependencies()
        {
            var ioc = new IocContainer(allowRecursiveResolution: true);
            ioc.Bind<IServiceLocator>(() => ioc);
            ioc.Bind<IExampleA, RecursiveNonCyclicA>();
            ioc.Bind<IExampleB, RecursiveNonCyclicB>();

            IExampleB resolved = ioc.Resolve<IExampleB>();
            Assert.IsInstanceOfType(resolved, typeof(RecursiveNonCyclicB));
        }


        interface IExampleA { }
        interface IExampleB { }

        class RecursiveNonCyclicA : IExampleA { }

        class RecursiveNonCyclicB : IExampleB
        {
            IExampleA a;

            public RecursiveNonCyclicB(IServiceLocator ioc)
            {
                // call GetInstance when we're already resolving this from the same container.
                a = ioc.GetInstance<IExampleA>();
            }
        }

        // Example classes for the `allowRecursiveResolution` constructor parameter.
        class ExampleA : IExampleA
        {
            private IExampleB b;

            public ExampleA(IServiceLocator ioc)
            {
                b = ioc.GetInstance<IExampleB>();
            }
        }

        class ExampleB : IExampleB
        {
            private IExampleA a;

            public ExampleB(IServiceLocator ioc)
            {
                a = ioc.GetInstance<IExampleA>();
            }
        }
    }
}
