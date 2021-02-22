using System;
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
        private readonly ReaderWriterLockSlim _lock;


        /// <summary>
        /// Initializes a new instance of <see cref="IocContainer"/>.
        /// </summary>
        public IocContainer()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="IocContainer"/>.
        /// </summary>
        /// <param name="allowRecursiveResolution">
        /// If true, recursive calls to Resolve or GetInstance overloads will be allowed.
        /// </param>
        /// <remarks>
        /// Recursive activations may cause a <see cref="StackOverflowException"/> if it
        /// triggers resolution of co-dependent types.
        /// 
        /// Care must be taken when enabling this feature. Consider the code:
        /// <code>
        /// class ExampleA : IExampleA
        /// {
        ///     private IExampleB b;
        ///     
        ///     public ExampleA(IServiceLocator ioc)
        ///     {
        ///         b = ioc.GetInstance&lt;IExampleB&gt;();
        ///     }
        /// }
        /// 
        /// class ExampleB : IExampleB
        /// {
        ///     private IExampleA a;
        ///     
        ///     public ExampleB(IServiceLocator ioc)
        ///     {
        ///         a = ioc.GetInstance&lt;IExampleA&gt;();
        ///     }
        /// }
        /// 
        /// void Main()
        /// {
        ///     var ioc = new IocContainer(<paramref name="allowRecursiveResolution"/>: false);
        ///     ioc.Bind&lt;IServiceLocator&gt;(() =&gt; ioc);
        ///     ioc.Bind&lt;IExampleA, ExampleA&gt;();
        ///     ioc.Bind&lt;IExampleB, ExampleB&gt;();
        ///     
        ///     IExampleA a = ioc.Resolve&lt;IExampleA&gt;();
        /// }
        /// </code>
        /// 
        /// If <paramref name="allowRecursiveResolution"/> were true, an uncatchable
        /// <see cref="StackOverflowException"/> would be raised because both ExampleA and
        /// ExampleB try to resolve each other from their constructors.
        /// </remarks>
        public IocContainer(bool allowRecursiveResolution)
            : this(null, allowRecursiveResolution)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="IocContainer"/> with a parent.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <remarks>
        /// If a service cannot be found in this container, the parent is checked.
        /// </remarks>
        public IocContainer(IServiceLocator parent)
            : this(parent, false)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="IocContainer"/> with a parent.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <param name="allowRecursiveResolution">
        /// If true, recursive calls to Resolve or GetInstance overloads will be allowed.
        /// </param>
        /// <remarks>
        /// If a service cannot be found in this container, the parent is checked.
        /// 
        /// Recursive activations may cause a <see cref="StackOverflowException"/> if it
        /// triggers resolution of co-dependent types.
        /// 
        /// Care must be taken when enabling this feature. Consider the code:
        /// <code>
        /// class ExampleA : IExampleA
        /// {
        ///     private IExampleB b;
        ///     
        ///     public ExampleA(IServiceLocator ioc)
        ///     {
        ///         b = ioc.GetInstance&lt;IExampleB&gt;();
        ///     }
        /// }
        /// 
        /// class ExampleB : IExampleB
        /// {
        ///     private IExampleA a;
        ///     
        ///     public ExampleB(IServiceLocator ioc)
        ///     {
        ///         a = ioc.GetInstance&lt;IExampleA&gt;();
        ///     }
        /// }
        /// 
        /// void Main()
        /// {
        ///     var ioc = new IocContainer(<paramref name="allowRecursiveResolution"/>: false);
        ///     ioc.Bind&lt;IServiceLocator&gt;(() =&gt; ioc);
        ///     ioc.Bind&lt;IExampleA, ExampleA&gt;();
        ///     ioc.Bind&lt;IExampleB, ExampleB&gt;();
        ///     
        ///     IExampleA a = ioc.Resolve&lt;IExampleA&gt;();
        /// }
        /// </code>
        /// 
        /// If <paramref name="allowRecursiveResolution"/> were true, an uncatchable
        /// <see cref="StackOverflowException"/> would be raised because both ExampleA and
        /// ExampleB try to resolve each other from their constructors.
        /// </remarks>
        public IocContainer(IServiceLocator parent, bool allowRecursiveResolution)
        {
            _parentContainer = parent;

            LockRecursionPolicy recursionPolicy = allowRecursiveResolution
                ? LockRecursionPolicy.SupportsRecursion
                : LockRecursionPolicy.NoRecursion;

            _lock = new ReaderWriterLockSlim(recursionPolicy);
        }


        /// <summary>
        /// Binds a configuration object for options type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <param name="options">The options values.</param>
        public void Configure<T>(T options)
        {
            Bind<IConfiguration<T>>(() => new SimpleConfiguration<T>(options), singleInstance: true);
        }

        /// <summary>
        /// Binds a configuration object for options type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The options type.</typeparam>
        /// <param name="key">A key to disambiguate between multiple instances of the same service.</param>
        /// <param name="options">The options values.</param>
        public void Configure<T>(string key, T options)
        {
            Bind<IConfiguration<T>>(key, () => new SimpleConfiguration<T>(options), singleInstance: true);
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
            BindCore(
                typeof(T), key,
                (context) => ResolveUnderLock(typeof(TClass), key, context),
                singleInstance
            );
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
        /// <param name="factory">A delegate which provides the service object.</param>
        /// <param name="singleInstance">
        /// If true, only a single instance of the service will be instantiated. Otherwise the 
        /// <paramref name="factory"/> will be invoked for each call to an overload of Resolve.
        /// </param>
        public void Bind<T>(Func<T> factory, bool singleInstance)
        {
            Bind(null, factory, singleInstance);
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

            BindCore(
                typeof(T), key,
                (context) => factory(),
                singleInstance
            );
        }

        private void BindCore(Type type, string key, Func<ResolutionContext, object> provider, bool singleInstance)
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

            try
            {
                // note: TryEnterReadLock still throws for recursive lock
                // calls if the lock doesn't support it.
                _lock.EnterReadLock();
            }
            catch (LockRecursionException)
            {
                throw new ActivationException(
                    "Attempted to recursively resolve an object from this container. If all "
                    + $"{nameof(Resolve)} calls are known to be acyclic, then try constructing"
                    + $"the {nameof(IocContainer)} with allowRecursiveResolution set to true."
                );
            }

            try
            {
                return ResolveUnderLock(type, key, new ResolutionContext());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private object ResolveUnderLock(Type type, string key, ResolutionContext context)
        {
            Debug.Assert(_lock.IsReadLockHeld);
            try
            {
                if (TryResolve(type, key, context, out BoundActivation bound))
                {
                    return bound.CreateObject(context);
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

        private bool TryResolve(Type type, string key, ResolutionContext context, out BoundActivation resolved)
        {
            Debug.Assert(_lock.IsReadLockHeld);

            var regKey = new RegistrationKey(type, key);
            if (context.WouldCreateCycle(regKey))
            {
                resolved = null;
                return false;
            }

            using (context.Activating(regKey))
            {
                // fast path: the type has a provider registered
                if (_registrations.TryGetValue(regKey, out var reg))
                {
                    resolved = new BoundActivation(reg);
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
                    BoundActivation[] parameters = new BoundActivation[pis.Length];
                    bool allBound = true;

                    for (int p = 0;
                        allBound && p < pis.Length;
                        p++)
                    {
                        string bindingKey = null;
                        BoundActivation paramProvider;

                        var bindingKeyAttr = pis[p].GetCustomAttribute<BindingKeyAttribute>();
                        if (!(bindingKeyAttr is null))
                        {
                            bindingKey = bindingKeyAttr.Key;
                        }

                        if (TryResolve(pis[p].ParameterType, bindingKey, context, out paramProvider))
                        {
                            // exact match with the binding key in this or parent.
                            parameters[p] = paramProvider;
                        }
                        else if (!(bindingKeyAttr is null)
                            && bindingKeyAttr.FallbackToDefaultBinding
                            && TryResolve(pis[p].ParameterType, null, context, out paramProvider))
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
                        var activation = new BoundActivation(ctor, parameters);
                        if (!activation.ContainsCycle())
                        {
                            resolved = activation;
                            return true;
                        }
                    }
                }
            }

            // we failed to bind a constructor here. If we have a parent, then defer to that container.
            if (!(_parentContainer is null))
            {
                return TryResolveFromParent(type, key, context, out resolved);
            }

            // no parent, so resolve failed
            resolved = null;
            return false;
        }

        private bool TryResolveFromParent(Type type, string key, ResolutionContext context, out BoundActivation registration)
        {
            Debug.Assert(_lock.IsReadLockHeld);
            Debug.Assert(!(_parentContainer is null));

            try
            {
                var service = _parentContainer.GetInstance(type, key);
                var reg = new Registration(type, key, (context) => service, RegistrationFlags.Implicit);
                registration = new BoundActivation(reg);
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
                    .Select(kvp => kvp.Value.GetValue(new ResolutionContext()))
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
    }
}
