using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Tests
{
    [TestClass]
    public class ValidationStrategiesTests
    {
        [TestMethod]
        public void TestImmediate()
        {
            IValidationStrategy strat = ValidationStrategies.Immediate;

            Assert.IsInstanceOfType(strat, typeof(ImmediateValidation));
        }

        [TestMethod]
        public void TestCached()
        {
            IValidationStrategy strat = ValidationStrategies.Cached;

            Assert.IsInstanceOfType(strat, typeof(CachedValidation));
        }
    }
}
