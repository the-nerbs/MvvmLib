using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    /// <summary>
    /// Tests that <see cref="IocContainer"/> implements the CommonServiceLocator contracts.
    /// </summary>
    /// <remarks>
    /// Reference the specifications here:
    /// https://github.com/unitycontainer/commonservicelocator/tree/d2b002d373c145eba7e64b832cd1304749f02800#specification
    /// </remarks>
    [TestClass]
    public class CommonServiceLocatorConformanceTests
    {
        [TestMethod]
        public void TestGetInstanceNullKey()
        {
            var ioc = new IocContainer();

            ioc.Bind<ITest, Concrete>();

            IServiceLocator locator = ioc;

            object service = locator.GetInstance(typeof(ITest), null);

            Assert.IsInstanceOfType(service, typeof(Concrete));
        }

        [TestMethod]
        public void TestGetInstanceExistingKey()
        {
            var ioc = new IocContainer();

            ioc.Bind<ITest, Concrete>("key");

            IServiceLocator locator = ioc;

            object service = locator.GetInstance(typeof(ITest), "key");

            Assert.IsInstanceOfType(service, typeof(Concrete));
        }

        [TestMethod]
        public void TestGetInstanceNonexistentKey()
        {
            var ioc = new IocContainer();

            ioc.Bind<ITest, Concrete>();
            ioc.Bind<ITest, Concrete>("key");

            IServiceLocator locator = ioc;

            Assert.ThrowsException<ActivationException>(
                () => locator.GetInstance(typeof(ITest), "wrong")
            );
        }


        [TestMethod]
        public void TestGetAllInstances()
        {
            var ioc = new IocContainer();

            var inst1 = new Concrete();
            var inst2 = new Concrete();

            ioc.Bind<ITest>(() => inst1);
            ioc.Bind<ITest>("key", () => inst2);

            IServiceLocator locator = ioc;

            object[] instances = locator.GetAllInstances(typeof(ITest)).ToArray();
            CollectionAssert.AreEqual(new[] { inst1, inst2 }, instances);
        }

        [TestMethod]
        public void TestGetAllInstancesWithNoneRegistered()
        {
            var ioc = new IocContainer();

            IServiceLocator locator = ioc;

            object[] instances = locator.GetAllInstances(typeof(ITest)).ToArray();
            CollectionAssert.AreEqual(Array.Empty<object>(), instances);
        }

        [TestMethod]
        public void TestGetAllInstancesActivationFailure()
        {
            var ioc = new IocContainer();

            ioc.Bind<ITest>(() => throw new Exception("test"));

            IServiceLocator locator = ioc;

            var ex = Assert.ThrowsException<ActivationException>(
                () => locator.GetAllInstances(typeof(ITest)).ToArray()
            );

            Assert.IsNotNull(ex.InnerException);
            Assert.AreEqual(typeof(Exception), ex.InnerException.GetType());
            Assert.AreEqual("test", ex.InnerException.Message);
        }


        [TestMethod]
        public void TestGetInstanceNoKey()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, Concrete>(singleInstance: true);

            IServiceLocator locator = ioc;

            object noKey = locator.GetInstance(typeof(ITest));
            object nullKey = locator.GetInstance(typeof(ITest), null);

            Assert.AreSame(noKey, nullKey);
        }

        [TestMethod]
        public void TestGenericGetInstanceNoKey()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, Concrete>(singleInstance: true);

            IServiceLocator locator = ioc;

            object generic = locator.GetInstance<ITest>();
            object nongeneric = locator.GetInstance(typeof(ITest), null);

            Assert.AreSame(generic, nongeneric);
        }

        [TestMethod]
        public void TestGenericGetInstanceWithKey()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, Concrete>("key", singleInstance: true);

            IServiceLocator locator = ioc;

            object generic = locator.GetInstance<ITest>("key");
            object nongeneric = locator.GetInstance(typeof(ITest), "key");

            Assert.AreSame(generic, nongeneric);
        }

        [TestMethod]
        public void TestGenericGetAllInstancesMatchesNonGeneric()
        {
            var ioc = new IocContainer();
            ioc.Bind<ITest, Concrete>(singleInstance: true);
            ioc.Bind<ITest, Concrete>("key1", singleInstance: true);
            ioc.Bind<ITest, Concrete>("key2", singleInstance: true);
            ioc.Bind<ITest, Concrete>("key3", singleInstance: true);

            IServiceLocator locator = ioc;

            ITest[] generic = locator.GetAllInstances<ITest>().ToArray();
            object[] nongeneric = locator.GetAllInstances(typeof(ITest)).ToArray();

            CollectionAssert.AreEqual(generic, nongeneric);
        }


        interface ITest
        { }

        class Concrete : ITest
        { }
    }
}
