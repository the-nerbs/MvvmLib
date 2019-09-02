using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Describes a validation rule.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Runs the validation rule.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>The result of the validation.</returns>
        /// <exception cref="InvalidCastException">The value has an unexpected type.</exception>
        ValidationRuleResult Run(object value);
    }

    /// <summary>
    /// Describes a validation rule.
    /// </summary>
    public interface IValidationRule<TValue> : IValidationRule
    {
        /// <summary>
        /// Runs the validation rule.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>The result of the validation.</returns>
        ValidationRuleResult Run(TValue value);
    }
}
