using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommonServiceLocator;

namespace MvvmLib.Ioc
{
    public class IocContainer
    {
        private readonly Dictionary<Type, Registration> _pregistrations = new Dictionary<Type, Registration>();


        public void Bind<T, TClass>()
            where TClass : class, T
        {
            Bind<T, TClass>(false);
        }

        public void Bind<T, TClass>(bool singleInstance)
            where TClass : class, T
        {
            BindCore(typeof(T), Resolve<TClass>, singleInstance);
        }

        public void Bind<T>(Func<T> factory)
        {
            Bind(factory, false);
        }

        public void Bind<T>(Func<T> factory, bool singleInstance)
        {
            Contract.RequiresNotNull(factory, nameof(factory));

            BindCore(typeof(T), () => factory, singleInstance);
        }

        private void BindCore(Type type, Func<object> provider, bool singleInstance)
        {
            RegistrationFlags flags = RegistrationFlags.None;

            if (singleInstance)
            {
                flags |= RegistrationFlags.SingleInstance;
            }

            _pregistrations[type] = new Registration(provider, flags);
        }


        public T Resolve<T>()
            where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            Contract.RequiresNotNull(type, nameof(type));

            if (TryResolve(type, out Registration regisration))
            {
                return regisration.GetValue();
            }

            throw new ActivationException($"Failed to resolve type {type}");
        }

        private bool TryResolve(Type type, out Registration resolved)
        {
            // fast path: the type has a provider registered
            if (_pregistrations.TryGetValue(type, out var reg))
            {
                resolved = reg;
                return true;
            }

            // there's nothing registered for the type - see if we can form an implicit
            // registration based on its constructors and what is registered.
            ConstructorInfo[] ctors = type.GetConstructors()
                .OrderByDescending(ci => ci.GetParameters().Length)
                .ToArray();

            for (int i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo ctor = ctors[i];
                ParameterInfo[] pis = ctor.GetParameters();
                Registration[] parameters = new Registration[pis.Length];
                bool allBound = true;

                for (int p = 0;
                    allBound && p < pis.Length;
                    p++)
                {
                    if (TryResolve(pis[p].ParameterType, out var paramProvider))
                    {
                        parameters[p] = paramProvider;
                    }
                    else
                    {
                        allBound = false;
                    }
                }

                if (allBound)
                {
                    object[] parameterValues = parameters.Select(paramProvider => paramProvider.GetValue()).ToArray();
                    resolved = new Registration(() => ctor.Invoke(parameterValues), RegistrationFlags.Implicit);
                    return true;
                }
            }

            resolved = null;
            return false;
        }
    }
}
