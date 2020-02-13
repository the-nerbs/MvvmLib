using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib.Tests
{
    [TestClass]
    public class ValidationUtilitiesTests
    {
        [TestMethod]
        public void TestValidateReturnsOnlyErrors()
        {
            var vm = new TestViewModel();
            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(vm.Property),
                new DelegateValidationRule<int>(x => new ValidationRuleResult(false, ""))
            ));
            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(vm.Property),
                new DelegateValidationRule<int>(x => new ValidationRuleResult(true, "error"))
            ));

            ValidationRuleResult[] results = ValidationUtilities.Validate(vm, nameof(vm.Property))
                .ToArray();

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(true, results[0].IsError);
            Assert.AreEqual("error", results[0].ErrorMessage);
        }

        [TestMethod]
        public void TestValidateGetValueFailure()
        {
            var vm = new TestViewModel();
            vm.GetterCache.Register(
                nameof(vm.Property),
                (vm) => throw new Exception("test")
            );

            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(vm.Property),
                new DelegateValidationRule<int>(x => new ValidationRuleResult(true, "error"))
            ));

            ValidationRuleResult[] results = ValidationUtilities.Validate(vm, nameof(vm.Property))
                .ToArray();

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(true, results[0].IsError);
            Assert.AreEqual("test", results[0].ErrorMessage);
        }

        [TestMethod]
        public void TestValidateRuleThrowsException()
        {
            var vm = new TestViewModel();

            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(vm.Property),
                new DelegateValidationRule<int>(x => throw new Exception("test exception"))
            ));
            vm.AllRules.Add(new KeyValuePair<string, IValidationRule>(
                nameof(vm.Property),
                new DelegateValidationRule<int>(x => new ValidationRuleResult(true, "error"))
            ));

            ValidationRuleResult[] results = ValidationUtilities.Validate(vm, nameof(vm.Property))
                .ToArray();

            Assert.AreEqual(2, results.Length);

            // first rule throws an exception
            Assert.AreEqual(true, results[0].IsError);
            Assert.AreEqual("test exception", results[0].ErrorMessage);

            // second rule yields a failed result
            Assert.AreEqual(true, results[1].IsError);
            Assert.AreEqual("error", results[1].ErrorMessage);
        }


        private class TestViewModel : IValidatingViewModel
        {
            public int Property { get; set; }

            public List<KeyValuePair<string, IValidationRule>> AllRules { get; }
                = new List<KeyValuePair<string, IValidationRule>>();

            public ILookup<string, IValidationRule> Rules
            {
                get { return AllRules.ToLookup(kvp => kvp.Key, kvp => kvp.Value); }
            }

            public TypeGetterCache GetterCache { get; } = new TypeGetterCache(typeof(TestViewModel));
        }
    }
}
