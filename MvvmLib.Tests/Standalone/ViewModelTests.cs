using System;
using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests.Standalone
{
    [TestClass]
    public class ViewModelTests
    {
        private class TestViewModel : ViewModel
        {
            new public IServiceLocator Services
            {
                get { return base.Services; }
            }


            public TestViewModel()
                : base()
            { }

            public TestViewModel(IServiceLocator services)
                : base(services)
            { }
        }

        private class Locator : ServiceLocatorImplBase
        {
            protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
            {
                throw new NotImplementedException();
            }

            protected override object DoGetInstance(Type serviceType, string key)
            {
                throw new NotImplementedException();
            }
        }

        // some of the tests here require using CommonServiceLocator's global default locator.
        // to avoid these tests conflicting with each other, lock on this.
        private static readonly object TestSyncLock = new object();


        [TestMethod]
        public void TestConstructWithServiceLocator()
        {
            var locator = new Locator();

            var vm = new TestViewModel(locator);

            Assert.AreSame(locator, vm.Services);
        }

        [TestMethod]
        public void TestConstructWithDefaultLocator()
        {
            lock (TestSyncLock)
            {
                var locator = new Locator();
                ServiceLocator.SetLocatorProvider(() => locator);

                try
                {
                    var vm = new TestViewModel();

                    Assert.AreSame(locator, vm.Services);
                }
                finally
                {
                    ServiceLocator.SetLocatorProvider(null);
                }
            }
        }

        [TestMethod]
        public void TestConstructWithDefaultLocatorFromNullArgument()
        {
            lock (TestSyncLock)
            {
                var locator = new Locator();
                ServiceLocator.SetLocatorProvider(() => locator);

                try
                {
                    var vm = new TestViewModel(null);

                    Assert.AreSame(locator, vm.Services);
                }
                finally
                {
                    ServiceLocator.SetLocatorProvider(null);
                } 
            }
        }
    }
}
