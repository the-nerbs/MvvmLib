using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CommonServiceLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests.Standalone
{
    [TestClass]
    public class ValidatingViewModelTests
    {
        private class TestViewModel : ValidatingViewModel
        {
            public int AProperty { get; set; }


            public TestViewModel()
                : base(new TestServiceLocator(new PropertyGetterCache()))
            { }

            public TestViewModel(int useDefaultBaseCtor)
                : base()
            { }


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
            try
            {
                var locator = new TestServiceLocator();
                ServiceLocator.SetLocatorProvider(() => locator);

                var vm = new TestViewModel(useDefaultBaseCtor: 1);

                Assert.AreSame(locator, vm.Services);
                Assert.AreSame(PropertyGetterCache.Default[typeof(TestViewModel)], vm.GetterCache);
                Assert.IsInstanceOfType(vm.ValidationStrategy, typeof(ImmediateValidation));
            }
            finally
            {
                ServiceLocator.SetLocatorProvider(null);
            }
        }


        [TestMethod]
        public void TestChangeValidationStrategy()
        {
            var vm = new TestViewModel();
            var strat = new TestStrategy();

            vm.ValidationStrategy = strat;

            Assert.AreSame(strat, vm.ValidationStrategy);
        }

        [TestMethod]
        public void TestChangeValidationStrategyToNull()
        {
            var vm = new TestViewModel();

            vm.ValidationStrategy = null;

            Assert.IsInstanceOfType(vm.ValidationStrategy, typeof(ImmediateValidation));
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

            List<string> errorChanges = CaptureErrorChanges(vm, () =>
            {
                vm.PublicRaisePropertyChanged(nameof(vm.AProperty));
            });

            CollectionAssert.AreEqual(
                new[] { nameof(TestViewModel.AProperty), string.Empty },
                errorChanges
            );
        }

        [TestMethod]
        public void TestPropertyChangedNullNameConvertedToEmpty()
        {
            var rule = new TestValidationRule
            {
                Result = ValidationRuleResult.Success,
            };

            var vm = new TestViewModel();

            vm.AddValidationRule(null, rule);

            List<string> errorChanges = CaptureErrorChanges(vm, () =>
            {
                vm.PublicRaisePropertyChanged(null);
            });

            CollectionAssert.AreEqual(
                new[] { string.Empty },
                errorChanges
            );
        }

        [TestMethod]
        public void TestPropertyChangeFiresErrorsChangedAndDoesNotThrowWithNoSubscribers()
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

            // this line should not throw any exceptions.
            vm.PublicRaisePropertyChanged(nameof(vm.AProperty));
        }


        [TestMethod]
        public void TestExplicitGetErrors()
        {
            var vm = new TestViewModel();
            vm.AddValidationRule<int>(nameof(vm.AProperty), x => new ValidationRuleResult(true, "error"));

            INotifyDataErrorInfo indei = vm;

            object[] indeiErrors = indei
                .GetErrors(nameof(vm.AProperty))
                .Cast<object>()
                .ToArray();

            string[] errors = vm.GetErrors(nameof(vm.AProperty)).ToArray();

            CollectionAssert.AreEqual(errors, indeiErrors);
        }


        private List<string> CaptureErrorChanges(ValidatingViewModel obj, Action action)
        {
            var changes = new List<string>();

            obj.ErrorsChanged += (sender, e) =>
            {
                changes.Add(e.PropertyName);
            };

            action();

            return changes;
        }


        private class TestStrategy : IValidationStrategy
        {
            public void Invalidate(string propertyName)
            {

            }

            public IEnumerable<ValidationRuleResult> Validate(IValidatingViewModel viewModel, string propertyName)
            {
                return Array.Empty<ValidationRuleResult>();
            }
        }
    }
}
