using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonServiceLocator;

namespace MvvmLib.Ioc
{
    public class IocContainer
    {
        private readonly Dictionary<Type, Func<object>> _providers = new Dictionary<Type, Func<object>>();


        public void Bind<T, TClass>()
            where TClass : class, T
        {
            _providers[typeof(T)] = Resolve<TClass>;
        }

        public void Bind<T>(Func<T> factory)
        {
            Contract.RequiresNotNull(factory, nameof(factory));

            _providers[typeof(T)] = (() => factory());
        }


        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            Contract.RequiresNotNull(type, nameof(type));

            if (TryResolve(type, out Func<object> provider))
            {
                return provider();
            }

            throw new ActivationException($"Failed to resolve type {type}");
        }

        private bool TryResolve(Type type, out Func<object> resolved)
        {
            // fast path: the type has a provider registered
            if (_providers.TryGetValue(type, out var provider))
            {
                resolved = provider;
                return true;
            }

            ConstructorInfo[] ctors = type.GetConstructors()
                .OrderByDescending(ci => ci.GetParameters().Length)
                .ToArray();

            for (int i = 0; i < ctors.Length; i++)
            {
                ConstructorInfo ctor = ctors[i];
                ParameterInfo[] pis = ctor.GetParameters();
                Func<object>[] parameters = new Func<object>[pis.Length];
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
                    object[] parameterValues = parameters.Select(paramProvider => paramProvider()).ToArray();
                    resolved = () => ctor.Invoke(parameterValues);
                    return true;
                }
            }

            resolved = null;
            return false;
        }
    }
}
