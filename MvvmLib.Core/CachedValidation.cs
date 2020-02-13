using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Validation strategy which caches the results for each property, re-validating the property
    /// any time it is invalidated.
    /// </summary>
    internal sealed class CachedValidation : IValidationStrategy
    {
        private readonly ConcurrentDictionary<string, IEnumerable<ValidationRuleResult>> _results
            = new ConcurrentDictionary<string, IEnumerable<ValidationRuleResult>>();


        /// <summary>
        /// Invalidates the validation results for the given property.
        /// </summary>
        /// <param name="propertyName">The name of the property, or the empty string for entity rules.</param>
        public void Invalidate(string propertyName)
        {
            _results.TryRemove(propertyName, out _);
        }

        /// <summary>
        /// Validates the given property.
        /// </summary>
        /// <param name="viewModel">The view model being validated.</param>
        /// <param name="propertyName">The name of the property being validated.</param>
        /// <returns>
        /// A collection of <see cref="ValidationRuleResult"/> objects describing validation
        /// failures. This does not include successful results.
        /// </returns>
        public IEnumerable<ValidationRuleResult> Validate(IValidatingViewModel viewModel, string propertyName)
        {
            IEnumerable<ValidationRuleResult> results;

            if (!_results.TryGetValue(propertyName, out results))
            {
                results = ValidationUtilities.Validate(viewModel, propertyName).ToArray();
                _results.TryAdd(propertyName, results);
            }

            return results;
        }
    }
}