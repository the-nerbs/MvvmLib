using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommonServiceLocator;

namespace MvvmLib.Ioc
{
    //TODO: implement IServiceLocator
    public class IocContainer 
    {
        private readonly Dictionary<RegistrationKey, Registration> _registrations = new Dictionary<RegistrationKey, Registration>();


        // Note: preferred parameter order for Bind methods here is:
        //  1. Binding key
        //  2. Object factory
        //  3. Single instance flag
        // There's no real reasoning behind this, just an order to keep them all consistent.

        public void Bind<T, TClass>()
            where TClass : class, T
        {
            Bind<T, TClass>(key: null);
        }

        public void Bind<T, TClass>(string key)
            where TClass : class, T
        {
            Bind<T, TClass>(key, false);
        }

        public void Bind<T, TClass>(bool singleInstance)
            where TClass : class, T
        {
            Bind<T, TClass>(null, singleInstance);
        }

        public void Bind<T, TClass>(string key, bool singleInstance)
            where TClass : class, T
        {
            BindCore(typeof(T), key, Resolve<TClass>, singleInstance);
        }

        public void Bind<T>(Func<T> factory)
        {
            Bind(null, factory);
        }

        public void Bind<T>(string key, Func<T> factory)
        {
            Bind(key, factory, false);
        }

        public void Bind<T>(string key, Func<T> factory, bool singleInstance)
        {
            Contract.RequiresNotNull(factory, nameof(factory));

            BindCore(typeof(T), key, () => factory(), singleInstance);
        }

        private void BindCore(Type type, string key, Func<object> provider, bool singleInstance)
        {
            RegistrationFlags flags = RegistrationFlags.None;

            if (singleInstance)
            {
                flags |= RegistrationFlags.SingleInstance;
            }

            // note: replaces any existing bindings.
            _registrations[new RegistrationKey(type, key)] = new Registration(provider, flags);
        }


        public T Resolve<T>()
            where T : class
        {
            return Resolve<T>(key: null);
        }

        public T Resolve<T>(string key)
            where T : class
        {
            return (T)Resolve(typeof(T), key);
        }

        public object Resolve(Type type)
        {
            return Resolve(type, key: null);
        }

        public object Resolve(Type type, string key)
        {
            Contract.RequiresNotNull(type, nameof(type));

            if (TryResolve(type, key, out Registration regisration))
            {
                return regisration.GetValue();
            }

            throw new ActivationException($"Failed to resolve type {type}");
        }

        private bool TryResolve(Type type, string key, out Registration resolved)
        {
            // fast path: the type has a provider registered
            if (_registrations.TryGetValue(new RegistrationKey(type, key), out var reg))
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
                    string bindingKey = null;
                    Registration paramProvider;

                    var bindingKeyAttr = pis[p].GetCustomAttribute<BindingKeyAttribute>();
                    if (!(bindingKeyAttr is null))
                    {
                        bindingKey = bindingKeyAttr.Key;
                    }

                    if (TryResolve(pis[p].ParameterType, bindingKey, out paramProvider))
                    {
                        // exact match with the binding key.
                        parameters[p] = paramProvider;
                    }
                    else if (!(bindingKeyAttr is null)
                        && bindingKeyAttr.FallbackToDefaultBinding
                        && TryResolve(pis[p].ParameterType, null, out paramProvider))
                    {
                        // exact match with the fallback key.
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


        readonly struct RegistrationKey : IEquatable<RegistrationKey>
        {
            public Type Type { get; }
            public string Key { get; }


            public RegistrationKey(Type type, string name)
            {
                Type = type;
                Key = name;
            }


            public bool Equals(RegistrationKey other)
            {
                return EqualityComparer<Type>.Default.Equals(Type, other.Type)
                    && StringComparer.Ordinal.Equals(Key, other.Key);
            }

            public override int GetHashCode()
            {
                // numbers generated by visual studio
                var hashCode = -1979447941;

                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);

                int nameHash = Key is null
                    ? 0
                    : StringComparer.Ordinal.GetHashCode(Key);
                hashCode = hashCode * -1521134295 + nameHash;

                return hashCode;
            }
        }
    }
}
