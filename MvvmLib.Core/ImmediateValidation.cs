using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib
{
    internal sealed class ImmediateValidation : IValidationStrategy
    {
        public void Invalidate(string propertyName)
        {
            // nothing to do
        }

        public IEnumerable<ValidationRuleResult> Validate(
            IValidatingViewModel viewModel, string propertyName)
        {
            return ValidationUtilities.Validate(viewModel, propertyName);
        }
    }
}
