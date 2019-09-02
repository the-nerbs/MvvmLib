using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Describes the results of a validation rule.
    /// </summary>
    public sealed class ValidationRuleResult
    {
        /// <summary>
        /// An instance of <see cref="ValidationRuleResult"/> describing a successful result.
        /// </summary>
        public static readonly ValidationRuleResult Success = new ValidationRuleResult(true, null);


        /// <summary>
        /// Gets a value indicating if the result is an error.
        /// </summary>
        public bool IsError { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ValidationRuleResult"/>.
        /// </summary>
        /// <param name="isError">A value indicating if this result is an error.</param>
        /// <param name="errorMessage">The error message.</param>
        public ValidationRuleResult(bool isError, string errorMessage)
        {
            IsError = isError;
            ErrorMessage = errorMessage;
        }
    }
}
