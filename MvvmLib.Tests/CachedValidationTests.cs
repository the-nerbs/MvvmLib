using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib.Tests
{
    [TestClass]
    public class CachedValidationTests
    {
        [TestMethod]
        [DataRow("AProperty")]
        [DataRow("")]
        public void TestInvalidateNames(string propertyName)
        {
            var strat = new CachedValidation();

            // "assert" that this does not throw
            strat.Invalidate(propertyName);
        }

        [TestMethod]
        public void TestInvalidate()
        {
            var strat = new CachedValidation();
            var vm = new TestViewModel();

            int callCount = 0;
            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(TestViewModel.TestProperty),
                new DelegateValidationRule<int>(x =>
                {
                    callCount++;
                    return new ValidationRuleResult(true, "error");
                })
            ));


            ValidationRuleResult[] r1 = strat.Validate(vm, nameof(TestViewModel.TestProperty)).ToArray();
            strat.Invalidate(nameof(TestViewModel.TestProperty));
            ValidationRuleResult[] r2 = strat.Validate(vm, nameof(TestViewModel.TestProperty)).ToArray();


            // assert that the rule was run once for each call
            Assert.AreEqual(2, callCount);

            // assert that the first run returned the expected results.
            Assert.AreEqual(1, r1.Length);
            Assert.AreEqual(true, r1[0].IsError);
            Assert.AreEqual("error", r1[0].ErrorMessage);

            // assert that the second run returned the expected results.
            Assert.AreEqual(1, r2.Length);
            Assert.AreEqual(true, r2[0].IsError);
            Assert.AreEqual("error", r2[0].ErrorMessage);
        }


        [TestMethod]
        public void TestValidate()
        {
            var strat = new CachedValidation();
            var vm = new TestViewModel();

            int callCount = 0;
            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(TestViewModel.TestProperty),
                new DelegateValidationRule<int>(x =>
                {
                    callCount++;
                    return new ValidationRuleResult(true, "error");
                })
            ));


            ValidationRuleResult[] r1 = strat.Validate(vm, nameof(TestViewModel.TestProperty)).ToArray();
            ValidationRuleResult[] r2 = strat.Validate(vm, nameof(TestViewModel.TestProperty)).ToArray();


            // assert that the rule was run once for each call
            Assert.AreEqual(1, callCount);

            // assert that the first run returned the expected results.
            Assert.AreEqual(1, r1.Length);
            Assert.AreEqual(true, r1[0].IsError);
            Assert.AreEqual("error", r1[0].ErrorMessage);

            // assert that the second run returned the expected results.
            Assert.AreEqual(1, r2.Length);
            Assert.AreEqual(true, r2[0].IsError);
            Assert.AreEqual("error", r2[0].ErrorMessage);
        }


        private class TestViewModel : IValidatingViewModel
        {
            public List<KeyValuePair<string, IValidationRule>> AllRules { get; } = new List<KeyValuePair<string, IValidationRule>>();

            public int TestProperty { get; set; }

            public ILookup<string, IValidationRule> Rules
            {
                get { return AllRules.ToLookup(r => r.Key, r => r.Value); }
            }

            public TypeGetterCache GetterCache { get; } = new TypeGetterCache(typeof(TestViewModel));
        }
    }
}
