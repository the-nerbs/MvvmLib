﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommonServiceLocator;

namespace MvvmLib.Ioc
{
    /// <summary>
    /// Another IOC container.
    /// </summary>
    public class IocContainer : IServiceLocator
    {
        private readonly Dictionary<RegistrationKey, Registration> _registrations = new Dictionary<RegistrationKey, Registration>();

        private readonly IServiceLocator _parentContainer;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();


        /// <summary>
        /// Initializes a new instance of <see cref="IocContainer"/>.
        /// </summary>
        public IocContainer()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="IocContainer"/> with a parent.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <remarks>
        /// If a service cannot be found in this container, the parent is checked.
        /// </remarks>
        public IocContainer(IServiceLocator parent)
        {
            _parentContainer = parent;
        }


        // Note: preferred parameter order for Bind methods here is:
        //  1. Binding key
        //  2. Object factory
        //  3. Single instance flag
        // There's no real reasoning behind this, just an order to keep them all consistent.

        /// <summary>
        /// Binds a service type to a particular implementation.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <typeparam name="TClass">The service implementation type.</typeparam>
        public void Bind<T, TClass>()
            where T : class
            where TClass : class, T
        {
            Bind<T, TClass>(key: null);
        }

        /// <summary>
        /// Binds a service type to a particular implementation.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <typeparam name="TClass">The service implementation type.</typeparam>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        public void Bind<T, TClass>(string key)
            where T : class
            where TClass : class, T
        {
            Bind<T, TClass>(key, false);
        }

        /// <summary>
        /// Binds a service type to a particular implementation.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <typeparam name="TClass">The service implementation type.</typeparam>
        /// <param name="singleInstance">
        /// If true, only a single instance of the service will be instantiated. Otherwise a new
        /// service instance will be created for each call to an overload of Resolve.
        /// </param>
        public void Bind<T, TClass>(bool singleInstance)
            where T : class
            where TClass : class, T
        {
            Bind<T, TClass>(null, singleInstance);
        }

        /// <summary>
        /// Binds a service type to a particular implementation.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <typeparam name="TClass">The service implementation type.</typeparam>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <param name="singleInstance">
        /// If true, only a single instance of the service will be instantiated. Otherwise a new
        /// service instance will be created for each call to an overload of Resolve.
        /// </param>
        public void Bind<T, TClass>(string key, bool singleInstance)
            where T : class
            where TClass : class, T
        {
            BindCore(typeof(T), key, () => ResolveUnderLock(typeof(TClass), key), singleInstance);
        }

        /// <summary>
        /// Binds a service type to an object yielded from a factory.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="factory">A delegate which provides the service object.</param>
        public void Bind<T>(Func<T> factory)
        {
            Bind(null, factory);
        }

        /// <summary>
        /// Binds a service type to an object yielded from a factory.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <param name="factory">A delegate which provides the service object.</param>
        public void Bind<T>(string key, Func<T> factory)
        {
            Bind(key, factory, false);
        }

        /// <summary>
        /// Binds a service type to an object yielded from a factory.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <param name="factory">A delegate which provides the service object.</param>
        /// <param name="singleInstance">
        /// If true, only a single instance of the service will be instantiated. Otherwise the 
        /// <paramref name="factory"/> will be invoked for each call to an overload of Resolve.
        /// </param>
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

            var regKey = new RegistrationKey(type, key);
            var registration = new Registration(type, key, provider, flags);

            _lock.EnterWriteLock();
            try
            {
                // note: replaces any existing bindings.
                _registrations[regKey] = registration;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Resolves a service object.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>An instance of the service.</returns>
        /// <exception cref="ActivationException">Failed to resolve the service instance.</exception>
        public T Resolve<T>()
            where T : class
        {
            return Resolve<T>(key: null);
        }

        /// <summary>
        /// Resolves a service object.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <returns>An instance of the service.</returns>
        /// <exception cref="ActivationException">Failed to resolve the service instance.</exception>
        public T Resolve<T>(string key)
            where T : class
        {
            return (T)Resolve(typeof(T), key);
        }

        /// <summary>
        /// Resolves a service object.
        /// </summary>
        /// <param name="type">The service type.</param>
        /// <returns>An instance of the service.</returns>
        /// <exception cref="ActivationException">Failed to resolve the service instance.</exception>
        public object Resolve(Type type)
        {
            return Resolve(type, key: null);
        }

        /// <summary>
        /// Resolves a service object.
        /// </summary>
        /// <param name="type">The service type.</param>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <returns>An instance of the service.</returns>
        /// <exception cref="ActivationException">Failed to resolve the service instance.</exception>
        public object Resolve(Type type, string key)
        {
            Contract.RequiresNotNull(type, nameof(type));

            _lock.EnterReadLock();
            try
            {
                return ResolveUnderLock(type, key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private object ResolveUnderLock(Type type, string key)
        {
            Debug.Assert(_lock.IsReadLockHeld);
            try
            {
                if (TryResolve(type, key, out Registration registration))
                {
                    return registration.GetValue();
                }

                throw new ActivationException($"Failed to resolve type {type}.");
            }
            catch (ActivationException)
            {
                // pass ActivationExceptions along as-is.
                throw;
            }
            catch (Exception ex)
            {
                // wrap other exceptions from TryResolve in an ActivationException
                throw new ActivationException($"Failed to resolve type {type}.", ex);
            }
        }

        private bool TryResolve(Type type, string key, out Registration resolved)
        {
            Debug.Assert(_lock.IsReadLockHeld);

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
                        // exact match with the binding key in this or parent.
                        parameters[p] = paramProvider;
                    }
                    else if (!(bindingKeyAttr is null)
                        && bindingKeyAttr.FallbackToDefaultBinding
                        && TryResolve(pis[p].ParameterType, null, out paramProvider))
                    {
                        // exact match with the fallback key in this or parent.
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
                    resolved = new Registration(type, key, () => ctor.Invoke(parameterValues), RegistrationFlags.Implicit);
                    return true;
                }
            }

            // we failed to bind a constructor here. If we have a parent, then defer to that container.
            if (!(_parentContainer is null))
            {
                return TryResolveFromParent(type, key, out resolved);
            }

            // no parent, so resolve failed
            resolved = null;
            return false;
        }

        private bool TryResolveFromParent(Type type, string key, out Registration registration)
        {
            Debug.Assert(_lock.IsReadLockHeld);
            Debug.Assert(!(_parentContainer is null));

            try
            {
                var service = _parentContainer.GetInstance(type, key);
                registration = new Registration(type, key, () => service, RegistrationFlags.Implicit);
                return true;
            }
            catch (ActivationException)
            {
                registration = default;
                return false;
            }
        }


        object IServiceProvider.GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        IEnumerable<object> IServiceLocator.GetAllInstances(Type serviceType)
        {
            _lock.EnterReadLock();
            try
            {
                // note: ToArray so we evaluate the enumeration while still under the lock.
                return _registrations
                    .Where(kvp => kvp.Key.Type == serviceType)
                    .Select(kvp => kvp.Value.GetValue())
                    .ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerable<TService> IServiceLocator.GetAllInstances<TService>()
        {
            return ((IServiceLocator)this).GetAllInstances(typeof(TService))
                .Cast<TService>();
        }

        object IServiceLocator.GetInstance(Type serviceType)
        {
            return ((IServiceLocator)this).GetInstance(serviceType, null);
        }

        object IServiceLocator.GetInstance(Type serviceType, string key)
        {
            return Resolve(serviceType, key);
        }

        TService IServiceLocator.GetInstance<TService>()
        {
            return (TService)((IServiceLocator)this).GetInstance(typeof(TService), null);
        }

        TService IServiceLocator.GetInstance<TService>(string key)
        {
            return (TService)((IServiceLocator)this).GetInstance(typeof(TService), key);
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
