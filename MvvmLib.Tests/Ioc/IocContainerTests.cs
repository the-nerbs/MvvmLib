using System;
using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    [TestClass]
    public class IocContainerTests
    {
        [TestMethod]
        public void TestResolveFromInterfaceBoundToConcreteType()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>();

            ITest obj = ioc.Resolve<ITest>();

            Assert.IsInstanceOfType(obj, typeof(ConcreteTest));
        }

        [TestMethod]
        public void TestResolveFromInterfaceBoundToConcreteTypeWithKey()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>("key");

            ITest obj = ioc.Resolve<ITest>("key");

            Assert.IsInstanceOfType(obj, typeof(ConcreteTest));
        }

        [TestMethod]
        public void TestResolveFromInterfaceBoundToFactory()
        {
            var ioc = new IocContainer();

            var test = new ConcreteTest();
            Func<ITest> factory = () => test;
            ioc.Bind<ITest>(factory);

            ITest obj = ioc.Resolve<ITest>();

            Assert.AreSame(test, obj);
        }

        [TestMethod]
        public void TestResolveFromInterfaceBoundToFactoryWithKey()
        {
            var ioc = new IocContainer();

            var test = new ConcreteTest();
            Func<ITest> factory = () => test;
            ioc.Bind<ITest>("key", factory);

            ITest obj = ioc.Resolve<ITest>("key");

            Assert.AreSame(test, obj);
        }

        [TestMethod]
        public void TestResolveObjectFromUnboundArguments()
        {
            var ioc = new IocContainer();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ObjectTakingITest>()
            );
        }

        [TestMethod]
        public void TestResolveObjectFromUnboundKeyedArguments()
        {
            var ioc = new IocContainer();

            // no key, so this should not bind successfully
            ioc.Bind<ITest, ConcreteTest>();

            // different key, so this should also not bind successfully
            ioc.Bind<ITest, ConcreteTest>("wrong key");

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ObjectTakingKeyedITest>()
            );
        }

        [TestMethod]
        public void TestResolveObjectFromFallbackKey()
        {
            var ioc = new IocContainer();

            // no key, but should bind successfully on the fallback check.
            ioc.Bind<ITest, ConcreteTest>();

            var obj = ioc.Resolve<ObjectTakingKeyedITestWithFallback>();

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestImplicitRegistrationCreatedForConstructibleType()
        {
            var ioc = new IocContainer();

            var test = new ConcreteTest();
            Func<ITest> factory = () => test;
            ioc.Bind<ITest>(factory);

            ObjectTakingITest obj = ioc.Resolve<ObjectTakingITest>();

            Assert.AreSame(test, obj.Test);
        }

        [TestMethod]
        public void TestSingleInstanceRegisrationResolvesSameInstance()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>(singleInstance:true);

            ITest test1 = ioc.Resolve<ITest>();
            ITest test2 = ioc.Resolve<ITest>();

            Assert.AreSame(test1, test2);
        }

        [TestMethod]
        public void TestNonSingleInstanceRegistrationResolvesNewInstance()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>(singleInstance: false);

            ITest test1 = ioc.Resolve<ITest>();
            ITest test2 = ioc.Resolve<ITest>();

            Assert.AreNotSame(test1, test2);
        }

        [TestMethod]
        public void TestResolveFromTypeObject()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>();

            var obj = ioc.Resolve(typeof(ITest));

            Assert.IsInstanceOfType(obj, typeof(ConcreteTest));
        }

        [TestMethod]
        public void TestResolveFromTypeObjectAndKey()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>("key");

            var obj = ioc.Resolve(typeof(ITest), "key");

            Assert.IsInstanceOfType(obj, typeof(ConcreteTest));
        }

        [TestMethod]
        public void TestResolveFromTypeMissingCtorParameterBinding()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, ConcreteTest>();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<ObjectTakingITestAndITest2>()
            );
        }


        interface ITest
        {
            void Test();
        }

        interface ITest2
        { }

        class ConcreteTest : ITest
        {
            public void Test()
            {
                throw new NotImplementedException();
            }
        }

        class ObjectTakingITest
        {
            public ITest Test { get; }

            public ObjectTakingITest(ITest test)
            {
                Test = test;
            }
        }

        class ObjectTakingKeyedITest
        {
            public ITest Test { get; }

            public ObjectTakingKeyedITest(
                [BindingKey("Key")] ITest test)
            {
                Test = test;
            }
        }

        class ObjectTakingKeyedITestWithFallback
        {
            public ITest Test { get; }

            public ObjectTakingKeyedITestWithFallback(
                [BindingKey("Key", true)] ITest test)
            {
                Test = test;
            }
        }

        class ObjectTakingITestAndITest2
        {
            public ObjectTakingITestAndITest2(ITest t1, ITest2 t2)
            { }
        }
    }
}
