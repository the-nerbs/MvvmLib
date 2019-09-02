using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;

namespace MvvmLib.MvvmLight
{
    /// <summary>
    /// Provides a base class for view models that validate their data.
    /// </summary>
    public abstract class ValidatingViewModel : ViewModel, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<IValidationRule>> _rules
            = new Dictionary<string, List<IValidationRule>>();


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
        protected TypeGetterCache GetterCache { get; }


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
            var cache = (PropertyGetterCache)Services.GetService(typeof(PropertyGetterCache));
            if (cache is null)
            {
                cache = PropertyGetterCache.Default;
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

            if (_rules.TryGetValue(propertyName, out var rules))
            {
                object value = default;
                Exception getValueError = null;

                if (string.IsNullOrEmpty(propertyName))
                {
                    value = this;
                }
                else
                {
                    Func<object, object> getter = GetterCache[propertyName];

                    try
                    {
                        value = getter(this);
                    }
                    catch (Exception ex)
                    {
                        getValueError = ex;
                    }
                }

                if (getValueError is null)
                {
                    foreach (IValidationRule rule in rules.ToArray())
                    {
                        ValidationRuleResult result;

                        try
                        {
                            result = rule.Run(value);
                        }
                        catch (Exception ex)
                        {
                            result = new ValidationRuleResult(true, ex.Message);
                        }

                        if (!(result is null)
                            && result.IsError)
                        {
                            yield return result.ErrorMessage;
                        }
                    }
                }
                else
                {
                    yield return getValueError.Message;
                }
            }
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return GetErrors(propertyName);
        }


        private static void PropertyChangedToErrorsChanged(object sender, PropertyChangedEventArgs e)
        {
            var vvm = (ValidatingViewModel)sender;

            if (vvm.ErrorsChanged is var handler)
            {
                handler(vvm, new DataErrorsChangedEventArgs(e.PropertyName));
                handler(vvm, new DataErrorsChangedEventArgs(string.Empty));
            }
        }
    }
}
