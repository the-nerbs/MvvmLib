using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Validation strategy which re-validates the property any time <see cref="Validate"/> is invoked.
    /// </summary>
    internal sealed class ImmediateValidation : IValidationStrategy
    {
        /// <summary>
        /// Invalidates a property's validation results.
        /// </summary>
        /// <param name="propertyName">The name of the property, or the empty string for entity rules.</param>
        public void Invalidate(string propertyName)
        {
            // nothing to do
        }

        /// <summary>
        /// Gets a collection of validation results
        /// </summary>
        /// <param name="viewModel">The view model whose property to validate.</param>
        /// <param name="propertyName">The name of the property, or the empty string for entity rules.</param>
        /// <returns>
        /// A collection of <see cref="ValidationRuleResult"/> objects describing validation
        /// failures. This does not include successful results.
        /// </returns>
        public IEnumerable<ValidationRuleResult> Validate(
            IValidatingViewModel viewModel, string propertyName)
        {
            return ValidationUtilities.Validate(viewModel, propertyName);
        }
    }
}
