using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Ioc
{
    /// <summary>
    /// Applied to a constructor parameter to indicate a key for the service to bind to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class BindingKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets the key used to resolve the parameter's dependency.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets a value indicating if, in the case that a dependency cannot be resolved using the 
        /// provided <see cref="Key"/>, then a second attempt should be made to resolve the
        /// dependency using the default (null) key.
        /// </summary>
        public bool FallbackToDefaultBinding { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="BindingKeyAttribute"/>.
        /// </summary>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        public BindingKeyAttribute(string key)
            : this(key, false)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="BindingKeyAttribute"/>.
        /// </summary>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <param name="fallbackToDefaultBinding">
        /// If true, then if the service cannot be resolved using the provided <see cref="Key"/>,
        /// a second attempt will be made to resolve the service using the default (null) key.
        /// </param>
        public BindingKeyAttribute(string key, bool fallbackToDefaultBinding)
        {
            Key = key;
            FallbackToDefaultBinding = fallbackToDefaultBinding;
        }
    }
}
