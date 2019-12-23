using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Provides helper functions for view model validation.
    /// </summary>
    public static class ValidationUtilities
    {
        /// <summary>
        /// Validates a property of a view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <returns>A collection of validation failures.</returns>
        public static IEnumerable<ValidationRuleResult> Validate(
            IValidatingViewModel viewModel, string propertyName)
        {
            IValidationRule[] rules = viewModel.Rules[propertyName].ToArray();
            object value = default;
            Exception getValueError = null;

            if (string.IsNullOrEmpty(propertyName))
            {
                value = viewModel;
            }
            else
            {
                Func<object, object> getter = viewModel.GetterCache[propertyName];

                try
                {
                    value = getter(viewModel);
                }
                catch (Exception ex)
                {
                    getValueError = ex;
                }
            }

            if (getValueError is null)
            {
                foreach (IValidationRule rule in rules.ToArray())
                {
                    ValidationRuleResult result;

                    try
                    {
                        result = rule.Run(value);
                    }
                    catch (Exception ex)
                    {
                        result = new ValidationRuleResult(true, ex.Message);
                    }

                    if (!(result is null)
                        && result.IsError)
                    {
                        yield return result;
                    }
                }
            }
            else
            {
                yield return new ValidationRuleResult(true, getValueError.Message);
            }
        }
    }
}
