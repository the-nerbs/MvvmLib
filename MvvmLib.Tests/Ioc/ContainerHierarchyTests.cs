using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    [TestClass]
    public class ContainerHierarchyTests
    {
        [TestMethod]
        public void TestResolveFromParentContainer()
        {
            var parent = new IocContainer();
            parent.Bind<ITest, Concrete>();

            var ioc = new IocContainer(parent);

            var obj = ioc.Resolve<ITest>();

            Assert.IsInstanceOfType(obj, typeof(Concrete));
        }

        [TestMethod]
        public void TestResolveFromGrandparentContainer()
        {
            var gp = new IocContainer();
            gp.Bind<ITest, Concrete>();

            var parent = new IocContainer(gp);

            var ioc = new IocContainer(parent);

            var obj = ioc.Resolve<ITest>();

            Assert.IsInstanceOfType(obj, typeof(Concrete));
        }

        [TestMethod]
        public void TestChildBindingOverridesParentBinding()
        {
            var parent = new IocContainer();

            ITest fromParent = new Concrete();
            parent.Bind<ITest>(()=>fromParent);

            var ioc = new IocContainer(parent);
            ITest fromChild = new Concrete();
            ioc.Bind<ITest>(() => fromChild);

            var obj = ioc.Resolve<ITest>();

            Assert.AreNotSame(fromParent, obj);
            Assert.AreSame(fromChild, obj);
        }

        [TestMethod]
        public void TestChildBindingDoesNotOverwriteParentContainerBinding()
        {
            var parent = new IocContainer();

            ITest fromParent = new Concrete();
            parent.Bind<ITest>(()=>fromParent);

            var ioc = new IocContainer(parent);
            ITest fromChild = new Concrete();
            ioc.Bind<ITest>(() => fromChild);

            var resolved = parent.Resolve<ITest>();
            Assert.AreNotSame(fromChild, resolved);
            Assert.AreSame(fromParent, resolved);
        }

        [TestMethod]
        public void TestParameterBindingFromParent()
        {
            var parent = new IocContainer();
            parent.Bind<ITest, Concrete>();

            var ioc = new IocContainer(parent);

            var resolved = ioc.Resolve<CtorTakesTest>();

            Assert.IsInstanceOfType(resolved, typeof(CtorTakesTest));
        }

        [TestMethod]
        public void TestParameterKeyedBindingFromParent()
        {
            var parent = new IocContainer();
            parent.Bind<ITest, Concrete>("key");

            var ioc = new IocContainer(parent);
            ioc.Bind<ITest, Concrete>(singleInstance: false);
            ioc.Bind<ITest, Concrete>("wrong", singleInstance: false);

            var resolved = ioc.Resolve<CtorTakesKeyedTest>();

            Assert.IsInstanceOfType(resolved, typeof(CtorTakesKeyedTest));

            // make sure we didn't take the default or wrong keyed instances from the child container.
            Assert.AreNotSame(ioc.Resolve<ITest>(), resolved.Test);
            Assert.AreNotSame(ioc.Resolve<ITest>("wrong"), resolved.Test);
        }

        [TestMethod]
        public void TestParameterBindingChecksParentForKeyBeforeFallingBackToDefaultBinding()
        {
            var parent = new IocContainer();
            parent.Bind<ITest, Concrete>("key", singleInstance: true);

            var ioc = new IocContainer(parent);
            ioc.Bind<ITest, Concrete>(singleInstance: true);

            var resolved = ioc.Resolve<CtorTakesKeyedTestWithFallback>();

            Assert.IsInstanceOfType(resolved, typeof(CtorTakesKeyedTestWithFallback));

            // make sure we got the keyed instance from the parent.
            Assert.AreSame(parent.Resolve<ITest>("key"), resolved.Test);
        }




        interface ITest
        { }

        class Concrete : ITest
        { }

        class CtorTakesTest
        {
            public ITest Test { get; }

            public CtorTakesTest(ITest test)
            {
                Test = test;
            }
        }

        class CtorTakesKeyedTest
        {
            public ITest Test { get; }

            public CtorTakesKeyedTest(
                [BindingKey("key")] ITest test)
            {
                Test = test;
            }
        }

        class CtorTakesKeyedTestWithFallback
        {
            public ITest Test { get; }

            public CtorTakesKeyedTestWithFallback(
                [BindingKey("key", true)] ITest test)
            {
                Test = test;
            }
        }
    }
}
