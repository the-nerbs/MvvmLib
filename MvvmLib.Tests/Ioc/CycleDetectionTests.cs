using System;
using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    [TestClass]
    public class CycleDetectionTests
    {
        [TestMethod]
        public void TestCycleInConcreteTypeCtors()
        {
            var ioc = new IocContainer();

            // cyclic with itself
            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<SelfCyclic>()
            );

            // cyclic with each other
            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<CyclicA>()
            );
            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<CyclicB>()
            );
        }

        [TestMethod]
        public void TestCycleInClassesBoundByInterfaceTakingClassTypes()
        {
            var ioc = new IocContainer();
            ioc.Bind<ICyclicA, CyclicA>();
            ioc.Bind<ICyclicB, CyclicB>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ICyclicA>()
            );

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ICyclicB>()
            );
        }

        [TestMethod]
        public void TestCycleInClassesBoundByInterfaceTakingInterfaceTypes()
        {
            var ioc = new IocContainer();
            ioc.Bind<ICyclicA, CyclicAThroughInterface>();
            ioc.Bind<ICyclicB, CyclicBThroughInterface>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ICyclicA>()
            );

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ICyclicB>()
            );
        }

        [TestMethod]
        public void TestCycleInClassesBoundByClass()
        {
            // Note: this is the case the case that is solved by Registration._activationStack.
            // Without that stack, this would result in a stack overflow instead of an
            // ActivationException that can be handled by client code.
            var ioc = new IocContainer();
            ioc.Bind<CyclicA, CyclicA>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<CyclicA>()
            );
        }

        [TestMethod]
        public void TestCycleInClassBoundByClassWithItself()
        {
            var ioc = new IocContainer();
            ioc.Bind<SelfCyclic, SelfCyclic>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<SelfCyclic>()
            );
        }

        [TestMethod]
        public void TestCycleInClassBoundByInterfaceWithItself()
        {
            var ioc = new IocContainer();
            ioc.Bind<ICyclicA, SelfCyclicThroughInterface>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<SelfCyclicThroughInterface>()
            );
        }


        interface ICyclicA
        { }

        interface ICyclicB
        { }

        // cyclic through the class types
        class CyclicA : ICyclicA
        {
            public CyclicA(CyclicB b)
            { }
        }
        class CyclicB : ICyclicB
        {
            public CyclicB(CyclicA a)
            { }
        }

        // maybe cyclic through the interfaces
        class CyclicAThroughInterface : ICyclicA
        {
            public CyclicAThroughInterface(ICyclicB b)
            { }
        }
        class CyclicBThroughInterface : ICyclicB
        {
            public CyclicBThroughInterface(ICyclicA a)
            { }
        }

        // cyclic through itself
        class SelfCyclic
        {
            public SelfCyclic(SelfCyclic a)
            { }
        }

        // maybe cyclic though the interface
        class SelfCyclicThroughInterface : ICyclicA
        {
            public SelfCyclicThroughInterface(ICyclicA a)
            { }
        }
    }
}
