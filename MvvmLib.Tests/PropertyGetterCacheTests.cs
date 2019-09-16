using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests
{
    [TestClass]
    public class PropertyGetterCacheTests
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void TestRegisterNull()
        {
            var cache = new PropertyGetterCache();

            Assert.ThrowsException<ArgumentNullException>(
                () => cache.Register(
                    typeof(PropertyGetterCacheTests),
                    nameof(TestContext),
                    null
                )
            );
        }

        [TestMethod]
        public void TestRegisterTNull()
        {
            var cache = new PropertyGetterCache();

            Assert.ThrowsException<ArgumentNullException>(
                () => cache.Register<PropertyGetterCacheTests>(
                    nameof(TestContext),
                    null
                )
            );
        }

        [TestMethod]
        public void TestRegisterAlreadyRegistered()
        {
            var cache = new PropertyGetterCache();

            cache.Register(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext),
                (obj) => 5
            );
            cache.Register(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext),
                (obj) => 7
            );

            var getter = cache.Get(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext)
            );

            Assert.AreEqual(7, getter(this));
        }


        [TestMethod]
        public void TestIndexer()
        {
            var cache = new PropertyGetterCache();
            cache.Register(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext),
                (obj) => 5
            );

            var typeCache = cache[typeof(PropertyGetterCacheTests)];

            Assert.IsNotNull(typeCache);
            Assert.AreEqual(typeof(PropertyGetterCacheTests), typeCache.Type);
        }

        [TestMethod]
        public void TestIndexerTypeNotRegistered()
        {
            var cache = new PropertyGetterCache();

            var typeCache = cache[typeof(PropertyGetterCacheTests)];
            Assert.IsNotNull(typeCache);
            Assert.AreEqual(typeof(PropertyGetterCacheTests), typeCache.Type);
        }


        [TestMethod]
        public void TestGet()
        {
            var cache = new PropertyGetterCache();
            cache.Register(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext),
                (obj) => 5
            );

            Func<object, object> getter = cache.Get(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext)
            );

            Assert.AreEqual(5, getter(this));
        }

        [TestMethod]
        public void TestGetNotRegistered()
        {
            var cache = new PropertyGetterCache();

            Func<object, object> getter = cache.Get(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext)
            );

            Assert.AreSame(TestContext, getter(this));
        }

        [TestMethod]
        public void TestGetRegisteredGeneric()
        {
            var cache = new PropertyGetterCache();
            cache.Register<PropertyGetterCacheTests>(
                nameof(TestContext),
                (obj) => 5
            );

            Func<object, object> getter = cache.Get(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext)
            );

            Assert.AreEqual(5, getter(this));
        }

        [TestMethod]
        public void TestGetRegisteredGenericFromTypesCache()
        {
            var cache = new PropertyGetterCache();

            cache[typeof(PropertyGetterCacheTests)].Register<PropertyGetterCacheTests>(
                nameof(TestContext),
                (obj) => 5
            );

            Func<object, object> getter = cache.Get(
                typeof(PropertyGetterCacheTests),
                nameof(TestContext)
            );

            Assert.AreEqual(5, getter(this));
        }
    }
}
