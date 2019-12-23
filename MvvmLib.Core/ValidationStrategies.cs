using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Provides a set of standard validation strategies.
    /// </summary>
    public static class ValidationStrategies
    {
        // Note: This is the only way our IValidationStrategy implementations are exposed publicly.
        // This means that this library controls how these are instantiated, so we don't let people
        // (for example) share instances of the CachedValidation when that is not a valid use.

        /// <summary>
        /// Gets a validation strategy that evaluates the validation rules each time the results
        /// are requested.
        /// </summary>
        public static IValidationStrategy Immediate { get; }
            = new ImmediateValidation();

        /// <summary>
        /// Gets a validation strategy that caches the validation results.
        /// </summary>
        public static IValidationStrategy Cached
        {
            get { return new CachedValidation(); }
        }
    }
}
