using System;
using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.MvvmLight;

namespace MvvmLib.Tests.MvvmLight
{
    [TestClass]
    public class ViewModelTests
    {
        private class TestViewModel : ViewModel
        {
            new public ISimpleIoc Services
            {
                get { return base.Services; }
            }


            public TestViewModel()
                : base()
            { }

            public TestViewModel(ISimpleIoc services)
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
            var locator = new SimpleIoc();

            var vm = new TestViewModel(locator);

            Assert.AreSame(locator, vm.Services);
        }

        [TestMethod]
        public void TestConstructWithDefaultLocator()
        {
            lock (TestSyncLock)
            {
                var locator = SimpleIoc.Default;

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
                var locator = SimpleIoc.Default;

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
