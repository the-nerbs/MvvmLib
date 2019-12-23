using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Ioc;

namespace MvvmLib.MvvmLight
{
    /// <summary>
    /// Provides a base class for view models that validate their data.
    /// </summary>
    public abstract class ValidatingViewModel : ViewModel, INotifyDataErrorInfo, IValidatingViewModel
    {
        private readonly Dictionary<string, List<IValidationRule>> _rules
            = new Dictionary<string, List<IValidationRule>>();

        private IValidationStrategy _validationStrategy = ValidationStrategies.Immediate;


        /// <summary>
        /// Occurs when a property's errors have changed.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;


        /// <summary>
        /// Gets a value indicating if this object has validation errors.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return _rules.SelectMany(kvp => GetErrors(kvp.Key)).Any();
            }
        }

        /// <summary>
        /// Gets a cache of delegates for accessing properties of this view model type.
        /// </summary>
        internal protected TypeGetterCache GetterCache { get; }

        /// <summary>
        /// Gets or sets the strategy used for property validation.
        /// </summary>
        internal protected IValidationStrategy ValidationStrategy
        {
            get { return _validationStrategy; }
            set { _validationStrategy = value ?? ValidationStrategies.Immediate; }
        }

        ILookup<string, IValidationRule> IValidatingViewModel.Rules
        {
            get
            {
                return _rules
                    .SelectMany(kvp => kvp.Value.Select(r => new KeyValuePair<string, IValidationRule>(kvp.Key, r)))
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        TypeGetterCache IValidatingViewModel.GetterCache
        {
            get { return GetterCache; }
        }


        /// <summary>
        /// Initializes a new view model using the default service locator.
        /// </summary>
        protected ValidatingViewModel()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new view model using the given service locator.
        /// </summary>
        /// <param name="services">The service locator. If null, the default will be used.</param>
        protected ValidatingViewModel(ISimpleIoc services)
            : base(services)
        {
            PropertyGetterCache cache = PropertyGetterCache.Default;
            if (Services.IsRegistered<PropertyGetterCache>())
            {
                cache = services.GetInstance<PropertyGetterCache>();
            }

            Type thisType = GetType();
            GetterCache = cache[thisType];

            PropertyChanged += PropertyChangedToErrorsChanged;
        }


        /// <summary>
        /// Adds a validation rule to a property.
        /// </summary>
        /// <typeparam name="T">The type of value being validated.</typeparam>
        /// <param name="propertyName">
        /// The name of the property to validate, or a null or empty string to validate the whole
        /// view model.
        /// </param>
        /// <param name="rule">The validation rule.</param>
        public void AddValidationRule<T>(string propertyName, Func<T, ValidationRuleResult> rule)
        {
            Contract.RequiresNotNull(rule, nameof(rule));

            AddValidationRule(propertyName, new DelegateValidationRule<T>(rule));
        }

        /// <summary>
        /// Adds a validation rule to a property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to validate, or a null or empty string to validate the whole
        /// view model.
        /// </param>
        /// <param name="rule">The validation rule.</param>
        public void AddValidationRule(string propertyName, IValidationRule rule)
        {
            Contract.RequiresNotNull(rule, nameof(rule));

            if (propertyName is null)
            {
                propertyName = string.Empty;
            }

            List<IValidationRule> propRules;

            if (!_rules.TryGetValue(propertyName, out propRules))
            {
                propRules = new List<IValidationRule>();
                _rules.Add(propertyName, propRules);
            }

            propRules.Add(rule);
        }

        /// <summary>
        /// Removes a validation rule from a property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to remove the validation rule from, or a null or empty string
        /// to remove the rule from the whole view model.
        /// </param>
        /// <param name="rule">The validation rule.</param>
        public void RemoveValidationRule(string propertyName, IValidationRule rule)
        {
            if (propertyName is null)
            {
                propertyName = string.Empty;
            }

            if (_rules.TryGetValue(propertyName, out List<IValidationRule> rules))
            {
                rules.Remove(rule);
            }
        }


        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to retrieve validation errors for; or null or
        /// <see cref="string.Empty"/> to retrieve entity-level errors.
        /// </param>
        /// <returns>The validation errors for the property or entity.</returns>
        public IEnumerable<string> GetErrors(string propertyName)
        {
            if (propertyName is null)
            {
                propertyName = string.Empty;
            }

            return _validationStrategy
                .Validate(this, propertyName)
                .Select(r => r.ErrorMessage);
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return GetErrors(propertyName);
        }


        /// <summary>
        /// Raises the <see cref="ErrorsChanged"/> event with the given property name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property whose errors may have changed, or null or empty to signal
        /// that the entity errors may have changed.
        /// </param>
        protected void RaiseErrorsChanged([CallerMemberName] string propertyName = null)
        {
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if (!(handler is null))
            {
                handler(this, new DataErrorsChangedEventArgs(propertyName));

                // if the given property name was for the entity, then don't raise
                // the event a second time for the entity specifically.
                if (!string.IsNullOrEmpty(propertyName))
                {
                    handler(this, new DataErrorsChangedEventArgs(string.Empty));
                }
            }
        }


        private static void PropertyChangedToErrorsChanged(object sender, PropertyChangedEventArgs e)
        {
            var vvm = (ValidatingViewModel)sender;

            string name = e.PropertyName ?? string.Empty;
            vvm._validationStrategy.Invalidate(name);
            vvm.RaiseErrorsChanged(name);
        }
    }
}
