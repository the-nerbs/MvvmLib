using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// A base class providing an implementation of <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Sets a property's backing field to a new value and raises a 
        /// <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The property's backing field type.</typeparam>
        /// <param name="field">A reference to the property's backing field.</param>
        /// <param name="value">The value to set the property to.</param>
        /// <param name="propertyName">The name of the property.</param>
        protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Sets a property's backing field to a new value and raises a
        /// <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The property's backing field type.</typeparam>
        /// <param name="field">A reference to the property's backing field.</param>
        /// <param name="value">The value to set the property to.</param>
        /// <param name="propertyName">The name of the property.</param>
        protected void SetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            SetIfChanged(ref field, value, null, propertyName);
        }

        /// <summary>
        /// Sets a property's backing field to a new value and raises a
        /// <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The property's backing field type.</typeparam>
        /// <param name="field">A reference to the property's backing field.</param>
        /// <param name="value">The value to set the property to.</param>
        /// <param name="comparer">
        /// The comparer used to check if the value has changed. If null, <see cref="EqualityComparer{T}.Default"/> is used.
        /// </param>
        /// <param name="propertyName">The name of the property.</param>
        protected void SetIfChanged<T>(
            ref T field, T value,
            IEqualityComparer<T> comparer,
            [CallerMemberName] string propertyName = null)
        {
            if (comparer is null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            if (!comparer.Equals(field, value))
            {
                Set(ref field, value, propertyName);
            }
        }
    }
}
