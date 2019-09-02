using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Provides a cache of delegates used to get the value of properties.
    /// </summary>
    /// <remarks>
    /// Instances of this type can be obtained through the <see cref="PropertyGetterCache"/> type.
    /// </remarks>
    public class TypeGetterCache
    {
        private readonly Dictionary<string, Func<object, object>> _getters
            = new Dictionary<string, Func<object, object>>();


        /// <summary>
        /// Gets the type this cache is for.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a getter for the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get a getter for.</param>
        /// <returns>A getter for the property.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if no getter delegate is registered for the property, and one could not be generated.
        /// </exception>
        public Func<object, object> this[string propertyName]
        {
            get
            {
                Func<object, object> getter;

                if (!_getters.TryGetValue(propertyName, out getter))
                {
                    getter = MakeDefaultGetter(propertyName);
                }

                return getter;
            }
        }


        internal TypeGetterCache(Type type)
        {
            Type = type;
        }


        /// <summary>
        /// Registers a delegate to get the value of a property from an object.
        /// </summary>
        /// <typeparam name="T">The type of object to get the value from.</typeparam>
        /// <param name="propertyName">The name of the property obtained by <paramref name="getter"/>.</param>
        /// <param name="getter">A delegate to get the value of the property.</param>
        public void Register<T>(string propertyName, Func<T, object> getter)
        {
            Contract.Requires<ArgumentException>(typeof(T) == Type, "T must match this cache's Type.");
            Contract.RequiresNotNull(getter, nameof(getter));

            Register(propertyName, (obj => getter((T)obj)));
        }

        /// <summary>
        /// Registers a delegate to get the value of a property from an object.
        /// </summary>
        /// <param name="propertyName">The name of the property obtained by <paramref name="getter"/>.</param>
        /// <param name="getter">A delegate to get the value of the property.</param>
        public void Register(string propertyName, Func<object, object> getter)
        {
            Contract.RequiresNotNull(propertyName, nameof(propertyName));
            Contract.RequiresNotNull(getter, nameof(getter));

            _getters[propertyName] = getter;
        }


        private Func<object, object> MakeDefaultGetter(string propertyName)
        {
            PropertyInfo propInfo = Type.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            Contract.Requires<ArgumentException>(
                !(propInfo is null),
                $"No property was found with the name {propertyName} on the type {Type.FullName}.",
                nameof(propertyName)
            );

            var vmParam = Expression.Parameter(typeof(object), "obj");

            // (object obj) => (object)(((Type)obj).Property)
            var expr = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.MakeMemberAccess(
                        Expression.Convert(vmParam, Type),
                        propInfo
                    ),
                    typeof(object)
                ),
                vmParam
            );

            return expr.Compile();
        }
    }
}
