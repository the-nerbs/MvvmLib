using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MvvmLib
{
    /// <summary>
    /// Provides a cache of delegates used to get properties from objects.
    /// </summary>
    public sealed class PropertyGetterCache
    {
        /// <summary>
        /// Gets a singleton instance of this type.
        /// </summary>
        public static readonly PropertyGetterCache Default = new PropertyGetterCache();


        private readonly Dictionary<Type, TypeGetterCache> _types
            = new Dictionary<Type, TypeGetterCache>();


        /// <summary>
        /// Gets a <see cref="TypeGetterCache"/> for the given type.
        /// </summary>
        /// <param name="type">The type whose cache to get.</param>
        /// <returns>A cache of delegates for a specific type.</returns>
        public TypeGetterCache this[Type type]
        {
            get
            {
                TypeGetterCache typeCache;

                if (!_types.TryGetValue(type, out typeCache))
                {
                    typeCache = new TypeGetterCache(type);
                    _types[type] = typeCache;
                }

                return typeCache;
            }
        }


        /// <summary>
        /// Gets a delegate which obtains the value of the property with the given name.
        /// </summary>
        /// <param name="type">The type the property belongs to.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>A delegate to get the value of the property.</returns>
        public Func<object, object> Get(Type type, string propertyName)
        {
            Contract.RequiresNotNull(type, nameof(type));
            Contract.RequiresNotNull(propertyName, nameof(propertyName));

            TypeGetterCache typeCache;

            if (!_types.TryGetValue(type, out typeCache))
            {
                typeCache = new TypeGetterCache(type);
                _types[type] = typeCache;
            }

            return typeCache[propertyName];
        }


        /// <summary>
        /// Registers a delegate to get the value of a property from an object.
        /// </summary>
        /// <typeparam name="T">The type of object to get the value from.</typeparam>
        /// <param name="propertyName">The name of the property obtained by <paramref name="getter"/>.</param>
        /// <param name="getter">A delegate to get the value of the property.</param>
        public void Register<T>(string propertyName, Func<T, object> getter)
        {
            Contract.RequiresNotNull(getter, nameof(getter));

            Register(typeof(T), propertyName, (obj => getter((T)obj)));
        }

        /// <summary>
        /// Registers a delegate to get the value of a property from an object.
        /// </summary>
        /// <param name="type">The type the property belongs to.</param>
        /// <param name="propertyName">The name of the property obtained by <paramref name="getter"/>.</param>
        /// <param name="getter">A delegate to get the value of the property.</param>
        public void Register(Type type, string propertyName, Func<object, object> getter)
        {
            Contract.RequiresNotNull(type, nameof(type));

            TypeGetterCache typeCache;

            if (!_types.TryGetValue(type, out typeCache))
            {
                typeCache = new TypeGetterCache(type);
                _types[type] = typeCache;
            }

            typeCache.Register(propertyName, getter);
        }
    }
}
