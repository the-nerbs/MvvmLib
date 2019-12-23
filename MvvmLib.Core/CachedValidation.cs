using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib
{
    internal sealed class CachedValidation : IValidationStrategy
    {
        private ConcurrentDictionary<string, IEnumerable<ValidationRuleResult>> _results
            = new ConcurrentDictionary<string, IEnumerable<ValidationRuleResult>>();


        public void Invalidate(string propertyName)
        {
            _results.TryRemove(propertyName, out _);
        }

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