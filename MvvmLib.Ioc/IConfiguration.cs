using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Ioc
{
    /// <summary>
    /// Provides a configuration structure.
    /// </summary>
    /// <typeparam name="TOptions">The configuration type.</typeparam>
    public interface IConfiguration<out TOptions>
    {
        /// <summary>
        /// Gets the configuration options.
        /// </summary>
        TOptions Value { get; }
    }
}
