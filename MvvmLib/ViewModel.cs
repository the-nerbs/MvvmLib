using System;
using System.ComponentModel;
using CommonServiceLocator;

namespace MvvmLib
{
    /// <summary>
    /// Provides a base class for view models.
    /// </summary>
    public abstract class ViewModel : ObservableObject
    {
        /// <summary>
        /// Gets an object that can be used to locate service objects.
        /// </summary>
        internal protected IServiceLocator Services { get; }


        /// <summary>
        /// Initializes a new view model using the default service locator.
        /// </summary>
        protected ViewModel()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new view model using the given service locator.
        /// </summary>
        /// <param name="services">The service locator. If null, the default will be used.</param>
        protected ViewModel(IServiceLocator services)
        {
            if (!ServiceLocator.IsLocationProviderSet)
            {
                Contract.RequiresNotNull(services, nameof(services));
            }

            Services = services ?? ServiceLocator.Current;
        }
    }
}
