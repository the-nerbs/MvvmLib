using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Interface for members that exist for each ValidatingViewModel implementation.
    /// </summary>
    public interface IValidatingViewModel
    {
        /// <summary>
        /// Gets a mapping from a property name to the collection of <see cref="IValidationRule"/>
        /// objects registered for that property.
        /// </summary>
        ILookup<string, IValidationRule> Rules { get; }

        /// <summary>
        /// Gets the getter cache.
        /// </summary>
        TypeGetterCache GetterCache { get; }
    }
}
