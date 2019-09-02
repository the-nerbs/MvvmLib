using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// A validation rule that invokes a delegate.
    /// </summary>
    /// <typeparam name="TValue">The context type.</typeparam>
    public sealed class DelegateValidationRule<TValue> : ValidationRule<TValue>
    {
        private readonly Func<TValue, ValidationRuleResult> _validator;


        /// <summary>
        /// Initializes a new instance of <see cref="DelegateValidationRule{TContext}"/>.
        /// </summary>
        /// <param name="validator">The validator delegate.</param>
        public DelegateValidationRule(Func<TValue, ValidationRuleResult> validator)
        {
            Contract.RequiresNotNull(validator, nameof(validator));

            _validator = validator;
        }


        /// <summary>
        /// Runs the validation rule.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>The result of the validation.</returns>
        public override ValidationRuleResult Run(TValue value)
        {
            return _validator(value);
        }
    }
}
