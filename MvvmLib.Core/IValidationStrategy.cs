using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Describes a validation strategy.
    /// </summary>
    public interface IValidationStrategy
    {
        /// <summary>
        /// Invalidates a property's validation results.
        /// </summary>
        /// <param name="propertyName">The name of the property, or the empty string for entity rules.</param>
        void Invalidate(string propertyName);

        /// <summary>
        /// Gets a collection of validation results
        /// </summary>
        /// <param name="viewModel">The view model whose property to validate.</param>
        /// <param name="propertyName">The name of the property, or the empty string for entity rules.</param>
        /// <returns>
        /// A collection of <see cref="ValidationRuleResult"/> objects describing validation
        /// failures. This does not include successful results.
        /// </returns>
        IEnumerable<ValidationRuleResult> Validate(IValidatingViewModel viewModel, string propertyName);
    }
}
