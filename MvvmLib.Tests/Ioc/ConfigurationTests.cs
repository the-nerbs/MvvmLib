using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.Ioc;

namespace MvvmLib.Tests.Ioc
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void TestResolveWithoutConfiguration()
        {
            var ioc = new IocContainer();

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<TestService>()
            );

            Assert.ThrowsException<ActivationException>(
                () => ioc.Resolve<TestServiceWithKeyedConfig>()
            );
        }

        [TestMethod]
        public void TestConfigure()
        {
            var ioc = new IocContainer();
            ioc.Configure(new TestOptions
            {
                IntValue = 5,
                StringValue = "TEST",
            });

            var svc = ioc.Resolve<TestService>();

            Assert.IsNotNull(svc.Configuration.Value);
            Assert.AreEqual(5, svc.Configuration.Value.IntValue);
            Assert.AreEqual("TEST", svc.Configuration.Value.StringValue);
        }

        [TestMethod]
        public void TestConfigureKeyedOptions()
        {
            var ioc = new IocContainer();
            ioc.Configure(new TestOptions
            {
                IntValue = 5,
                StringValue = "TEST",
            });
            ioc.Configure("opt2", new TestOptions
            {
                IntValue = 123,
                StringValue = "OTHER TEST"
            });

            var svc = ioc.Resolve<TestServiceWithKeyedConfig>();

            Assert.IsNotNull(svc.Configuration.Value);
            Assert.AreEqual(123, svc.Configuration.Value.IntValue);
            Assert.AreEqual("OTHER TEST", svc.Configuration.Value.StringValue);
        }


        class TestOptions
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
        }

        class TestService
        {
            public IConfiguration<TestOptions> Configuration { get; }

            public TestService(IConfiguration<TestOptions> config)
            {
                Configuration = config;
            }
        }

        class TestServiceWithKeyedConfig
        {
            public IConfiguration<TestOptions> Configuration { get; }

            public TestServiceWithKeyedConfig(
                [BindingKey("opt2")] IConfiguration<TestOptions> config)
            {
                Configuration = config;
            }
        }
    }
}
