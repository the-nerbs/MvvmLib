using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MvvmLib
{
    /// <summary>
    /// Tells FxCop that a parameter is validated as non-null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ValidatedNotNullAttribute : Attribute { }

    /// <summary>
    /// Mimics the Code Contracts preconditions API, but does not require the code rewrite tool.
    /// Since this does not use the rewrite tool, the post-conditions API is not available.
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// Indicates a pre-condition on the given value not being null/
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void RequiresNotNull<T>([ValidatedNotNull] T value, string paramName)
             where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Indicates a pre-condition on the given value not being null/
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void RequiresNotNull<T>([ValidatedNotNull] T? value, string paramName)
             where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }


        /// <summary>
        /// Indicates a precondition. If the condition fails, an exception of type TException is thrown.
        /// </summary>
        /// <typeparam name="TException">The exception to throw if the condition fails.</typeparam>
        /// <param name="condition">The condition.</param>
        /// <param name="args">The parameters to construct the exception with.</param>
        public static void Requires<TException>(bool condition, params object[] args)
            where TException : Exception, new()
        {
            if (!condition)
            {
                throw CreateException<TException>(args);
            }
        }


        /// <summary>
        /// A precondition where a provided enum argument must be a defined value, and not, for
        /// example, a bitwise combination of values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void RequiresDefinedValue<TEnum>(TEnum value, string paramName)
            where TEnum : struct, IConvertible
        {
            Debug.Assert(typeof(TEnum).IsEnum);

            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                throw InvalidEnum(value, paramName);
            }
        }


        /// <summary>
        /// A helper for more concisely creating <see cref="InvalidEnumArgumentException"/> instances.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum value.</typeparam>
        /// <param name="value">The invalid enum value.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <returns>An <see cref="InvalidEnumArgumentException"/> instance.</returns>
        /// <remarks>
        /// The typical use case for this function would be a switch-case in which all defined
        /// values have cases, such as the following:
        /// <code>
        ///     enum Foo { A, B, C }
        ///
        ///     void DoTheThing(Foo foo)
        ///     {
        ///         switch (foo)
        ///         {
        ///             case Foo.A:
        ///                 A();
        ///                 break;
        ///
        ///             case Foo.B:
        ///             case Foo.C:
        ///                 BOrC();
        ///                 break;
        ///
        ///             default:
        ///                 throw Contract.InvalidEnum(foo, nameof(foo));
        ///         }
        ///     }
        /// </code>
        /// </remarks>
        public static InvalidEnumArgumentException InvalidEnum<TEnum>(TEnum value, string paramName)
            where TEnum : struct, IConvertible
        {
            Debug.Assert(typeof(TEnum).IsEnum);

            return new InvalidEnumArgumentException(paramName, value.ToInt32(null), typeof(TEnum));
        }


        /// <summary>
        /// Used to signal that a code path should never be hit under normal execution.
        /// </summary>
        /// <param name="message">A message describing why the code path is not reachable.</param>
        /// <returns>An exception for the caller to throw.</returns>
        public static Exception UnreachableCode([Localizable(false)] string message)
        {
            Trace.TraceError(message);

            return new UnreachableCodeException(message);
        }


        /// <summary>
        /// Throws an exception of type TException.
        /// </summary>
        /// <typeparam name="TException">The exception type to throw.</typeparam>
        /// <param name="args">The parameters to construct the exception with.</param>
        private static TException CreateException<TException>(object[] args = null)
            where TException : Exception, new()
        {
            // invoke the constructor which takes a message or parameter name if we can.
            if (args != null)
            {
                return (TException)Activator.CreateInstance(typeof(TException), args);
            }
            else
            {
                return Activator.CreateInstance<TException>();
            }
        }


        [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic",
            Justification = "This exception is intentionally private since it should never be caught by consuming code.")]
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
            Justification = "This exception type is a debugging tool, and should never be raised by well-functioning code.")]
        private class UnreachableCodeException : Exception
        {
            public UnreachableCodeException(string message)
                : base(message)
            { }
        }
    }
}
