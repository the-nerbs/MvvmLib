using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using CommonServiceLocator;

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
        /// The registration was implicitly defined by an overload of IocContainer.Resolve.
        /// </summary>
        Implicit        = (1 << 1),
    }

    internal sealed class Registration
    {
        private object _value;

        public Type ServiceType { get; }
        public string Key { get; }
        public RegistrationFlags Flags { get; }

        public bool IsSingleInstance
        {
            get { return HasFlag(Flags, RegistrationFlags.SingleInstance); }
        }

        public bool IsImplicit
        {
            get { return HasFlag(Flags, RegistrationFlags.Implicit); }
        }


        public Registration(Type serviceType, string key, Func<ResolutionContext, object> provider, RegistrationFlags flags)
        {
            Debug.Assert(!(serviceType is null));

            ServiceType = serviceType;
            Key = key;
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


        public object GetValue(ResolutionContext context)
        {
            try
            {
                if (IsSingleInstance)
                {
                    return GetSingletonValue(context);
                }

                return DefaultGetValue(context);
            }
            catch (ActivationException)
            {
                // throw ActivationExceptions along as-is
                throw;
            }
            catch (Exception ex)
            {
                string message;
                if (!(Key is null))
                {
                    message = $"Activation error occurred while trying to get instance of type {ServiceType.Name}, key \"{Key}\"";
                }
                else
                {
                    message = $"Activation error occurred while trying to get instance of type {ServiceType}";
                }
                throw new ActivationException(message, ex);
            }
        }

        private object GetSingletonValue(ResolutionContext context)
        {
            // create the singleton value. Note that Lazy<T> uses thread-safe initialization.
            if (_value is LazyLoader loader)
            {
                _value = loader.GetValue(context);
            }

            return _value;
        }

        private object DefaultGetValue(ResolutionContext context)
        {
            Debug.Assert(_value is Func<ResolutionContext, object>);
            using (context.Activating(new RegistrationKey(ServiceType, Key)))
            {
                return ((Func<ResolutionContext, object>)_value)(context);
            }
        }


        private static bool HasFlag(RegistrationFlags flags, RegistrationFlags value)
        {
            return ((flags & value) == value);
        }


        private class LazyLoader
        {
            const int State_None = 0;
            const int State_Activating = 1;
            const int State_Activated = 2;
            const int State_Failed = 3;

            private readonly Func<ResolutionContext, object> _provider;
            private int _state;
            private object _obj;
            private Exception _failure;


            public LazyLoader(Func<ResolutionContext, object> provider)
            {
                Debug.Assert(provider != null);

                _provider = provider;
                _state = State_None;
                _obj = null;
            }

            public object GetValue(ResolutionContext context)
            {
                // loop until a terminal state is reached.
                while (true)
                {
                    int result = Interlocked.CompareExchange(ref _state, State_Activating, State_None);

                    switch (result)
                    {
                        case State_None:
                            // we create
                            try
                            {
                                _obj = _provider(context);
                                _state = State_Activated;
                            }
                            catch (Exception ex)
                            {
                                _failure = ex;
                                _state = State_Failed;
                            }
                            break;

                        case State_Activating:
                            // someone else is creating, so wait
                            if (!SpinWait.SpinUntil(
                                () => Volatile.Read(ref _state) != State_Activating,
                                TimeSpan.FromSeconds(5)))
                            {
                                _failure = new TimeoutException($"Timeout while waiting for singleton ");
                                _state = State_Failed;
                            }
                            break;

                        case State_Activated:
                            return _obj;

                        case State_Failed:
                            throw new ActivationException("Activation failed.", _failure);

                        default:
                            throw Contract.UnreachableCode("unexpected state");
                    }
                }
            }
        }
    }
}
