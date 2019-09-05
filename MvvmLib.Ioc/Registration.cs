using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MvvmLib.Ioc
{
    [Flags]
    internal enum RegistrationFlags
    {
        /// <summary>
        /// Nothing special about the registration.
        /// </summary>
        None = 0,

        /// <summary>
        /// The registration should always provide the same instance.
        /// </summary>
        SingleInstance  = (1 << 0),

        /// <summary>
        /// The registration was implicitly defined by an overload of
        /// <see cref="IocContainer.Resolve"/>.
        /// </summary>
        Implicit        = (1 << 1),
    }

    internal sealed class Registration
    {
        private object _value;


        public RegistrationFlags Flags { get; }

        public bool IsSingleInstance
        {
            get { return HasFlag(Flags, RegistrationFlags.SingleInstance); }
        }

        public bool IsImplicit
        {
            get { return HasFlag(Flags, RegistrationFlags.Implicit); }
        }


        public Registration(Func<object> provider, RegistrationFlags flags)
        {
            Flags = flags;

            if (IsSingleInstance)
            {
                _value = new LazyLoader(provider);
            }
            else
            {
                _value = provider;
            }
        }


        public object GetValue()
        {
            if (IsSingleInstance)
            {
                return GetSingletonValue();
            }

            return DefaultGetValue();
        }

        private object GetSingletonValue()
        {
            // create the singleton value. Note that Lazy<T> uses thread-safe
            if (_value is LazyLoader loader)
            {
                _value = loader.initializer.Value;
            }

            return _value;
        }

        private object DefaultGetValue()
        {
            Debug.Assert(_value is Func<object>);
            return ((Func<object>)_value)();
        }


        private static bool HasFlag(RegistrationFlags flags, RegistrationFlags value)
        {
            return ((flags & value) == value);
        }

        private class LazyLoader
        {
            public readonly Lazy<object> initializer;

            public LazyLoader(Func<object> provider)
            {
                initializer = new Lazy<object>(provider);
            }
        }
    }
}
