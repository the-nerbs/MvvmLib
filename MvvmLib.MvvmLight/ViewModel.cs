using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace MvvmLib.MvvmLight
{
    /// <summary>
    /// Provides a base class for view models.
    /// </summary>
    public abstract class ViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets an object that can be used to locate service objects.
        /// </summary>
        protected ISimpleIoc Services { get; }


        /// <summary>
        /// Initializes a new view model using the default service container.
        /// </summary>
        protected ViewModel()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new view model using the given service container.
        /// </summary>
        /// <param name="services">The service container. If null, the default will be used.</param>
        protected ViewModel(ISimpleIoc services)
        {
            Services = services ?? SimpleIoc.Default;
        }
    }
}
