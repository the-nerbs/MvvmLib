using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmLib.MvvmLight;

namespace MvvmLib.Tests.MvvmLight
{
    [TestClass]
    public class ValidatingViewModelTests
    {
        private class TestViewModel : ValidatingViewModel
        {
            public int AProperty { get; set; }


            public TestViewModel()
                : base(DefaultServices())
            { }

            public TestViewModel(int useDefaultBaseCtor)
                : base()
            { }


            private static ISimpleIoc DefaultServices()
            {
                var container = new SimpleIoc();
                container.Register(() => new PropertyGetterCache());
                return container;
            }


            public void PublicRaisePropertyChanged(string name)
            {
                RaisePropertyChanged(name);
            }
        }

        private class TestValidationRule : IValidationRule
        {
            public ValidationRuleResult Result { get; set; }

            public int RunCount { get; private set; }
            public object LastRunValue { get; private set; }

            public ValidationRuleResult Run(object value)
            {
                RunCount++;
                LastRunValue = value;

                return Result;
            }
        }


        [TestMethod]
        public void TestDefaultState()
        {
            var vm = new TestViewModel(useDefaultBaseCtor: 1);

            Assert.AreSame(SimpleIoc.Default, vm.Services);
            Assert.AreSame(PropertyGetterCache.Default[typeof(TestViewModel)], vm.GetterCache);
        }


        [TestMethod]
        public void TestValidatePropertyWithError()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(nameof(TestViewModel.AProperty), rule);

            string[] errors = vm.GetErrors(nameof(TestViewModel.AProperty)).ToArray();

            Assert.AreEqual(1, rule.RunCount);
            Assert.AreEqual(5, rule.LastRunValue);
            CollectionAssert.AreEqual(new string[] { "test" }, errors);
        }

        [TestMethod]
        public void TestValidatePropertyWithSuccess()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(false, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(nameof(TestViewModel.AProperty), rule);

            string[] errors = vm.GetErrors(nameof(TestViewModel.AProperty)).ToArray();

            Assert.AreEqual(1, rule.RunCount);
            Assert.AreEqual(5, rule.LastRunValue);
            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }


        [TestMethod]
        public void TestValidateEntityUsingNullStringWithError()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(null, rule);

            string[] errors = vm.GetErrors(null).ToArray();

            CollectionAssert.AreEqual(new string[] { "test" }, errors);
        }

        [TestMethod]
        public void TestValidateEntityUsingNullStringWithSuccess()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(false, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(null, rule);

            string[] errors = vm.GetErrors(null).ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }


        [TestMethod]
        public void TestValidateEntityUsingEmptyStringWithError()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(string.Empty, rule);

            string[] errors = vm.GetErrors(string.Empty).ToArray();

            CollectionAssert.AreEqual(new string[] { "test" }, errors);
        }

        [TestMethod]
        public void TestValidateEntityUsingEmptyStringWithSuccess()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(false, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(string.Empty, rule);

            string[] errors = vm.GetErrors(string.Empty).ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }


        [TestMethod]
        public void TestValidateEntityUsingEmptyAndNullStringWithError()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(string.Empty, rule);

            string[] errors = vm.GetErrors(null).ToArray();

            CollectionAssert.AreEqual(new string[] { "test" }, errors);
        }

        [TestMethod]
        public void TestValidateEntityUsingEmptyAndNullStringWithSuccess()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(false, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(string.Empty, rule);

            string[] errors = vm.GetErrors(null).ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }


        [TestMethod]
        public void TestValidateEntityUsingNullAndEmptyStringWithError()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(null, rule);

            string[] errors = vm.GetErrors(string.Empty).ToArray();

            CollectionAssert.AreEqual(new string[] { "test" }, errors);
        }

        [TestMethod]
        public void TestValidateEntityUsingNullAndEmptyStringWithSuccess()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(false, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(null, rule);

            string[] errors = vm.GetErrors(string.Empty).ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }


        [TestMethod]
        public void TestAddPropertyValidationRuleByDelegate()
        {
            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            int runCount = 0;

            vm.AddValidationRule<int>(nameof(TestViewModel.AProperty), (testVM) =>
            {
                runCount++;
                return new ValidationRuleResult(true, "test");
            });

            string[] errors = vm.GetErrors(nameof(TestViewModel.AProperty)).ToArray();

            Assert.AreEqual(1, runCount);
            CollectionAssert.AreEqual(new[] { "test" }, errors);
        }

        [TestMethod]
        public void TestAddEntityValidationRuleByDelegate()
        {
            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            int runCount = 0;

            vm.AddValidationRule<TestViewModel>(null, (testVM) =>
            {
                runCount++;
                return new ValidationRuleResult(true, "test");
            });

            string[] errors = vm.GetErrors(null).ToArray();

            Assert.AreEqual(1, runCount);
            CollectionAssert.AreEqual(new[] { "test" }, errors);
        }


        [TestMethod]
        public void TestRemovePropertyValidationRule()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(nameof(TestViewModel.AProperty), rule);

            vm.RemoveValidationRule(nameof(TestViewModel.AProperty), rule);

            string[] errors = vm.GetErrors(nameof(TestViewModel.AProperty)).ToArray();

            Assert.AreEqual(0, rule.RunCount);
            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }

        [TestMethod]
        public void TestRemoveEntityValidationRule()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(null, rule);

            vm.RemoveValidationRule(null, rule);

            string[] errors = vm.GetErrors(null).ToArray();

            Assert.AreEqual(0, rule.RunCount);
            CollectionAssert.AreEqual(Array.Empty<string>(), errors);
        }


        [TestMethod]
        public void TestHasErrorsTrue()
        {
            var rule = new TestValidationRule
            {
                Result = new ValidationRuleResult(true, "test"),
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(nameof(TestViewModel.AProperty), rule);

            Assert.IsTrue(vm.HasErrors);
        }

        [TestMethod]
        public void TestHasErrorsFalse()
        {
            var rule = new TestValidationRule
            {
                Result = ValidationRuleResult.Success,
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(nameof(TestViewModel.AProperty), rule);

            Assert.IsFalse(vm.HasErrors);
        }


        [TestMethod]
        public void TestPropertyChangeFiresErrorsChanged()
        {
            var rule = new TestValidationRule
            {
                Result = ValidationRuleResult.Success,
            };

            var vm = new TestViewModel
            {
                AProperty = 5,
            };

            vm.AddValidationRule(nameof(TestViewModel.AProperty), rule);

            var errorChanges = new List<string>();
            vm.ErrorsChanged += (sender, e) =>
            {
                errorChanges.Add(e.PropertyName);
            };

            vm.PublicRaisePropertyChanged(nameof(vm.AProperty));

            CollectionAssert.AreEqual(
                new[] { nameof(TestViewModel.AProperty), string.Empty },
                errorChanges
            );
        }
    }
}
