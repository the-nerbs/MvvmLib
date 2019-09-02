using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Provides a base class for validation rules.
    /// </summary>
    /// <typeparam name="TValue">The type of value to validate.</typeparam>
    public abstract class ValidationRule<TValue> : IValidationRule<TValue>
    {
        /// <summary>
        /// Runs the validation rule.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>The result of the validation.</returns>
        public abstract ValidationRuleResult Run(TValue value);


        ValidationRuleResult IValidationRule.Run(object value)
        {
            return Run((TValue)value);
        }
    }
}
