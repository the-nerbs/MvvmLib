using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonServiceLocator;

namespace MvvmLib.Tests.Standalone
{
    sealed class TestServiceLocator : ServiceLocatorImplBase
    {
        private Dictionary<ServiceKey, object> _services = new Dictionary<ServiceKey, object>();


        public TestServiceLocator()
        { }

        public TestServiceLocator(params object[] services)
        {
            foreach (var svc in services)
            {
                Register(svc.GetType(), svc);
            }
        }


        public void Register<T>(T service)
        {
            Register(service, null);
        }

        public void Register<T>(T service, string key)
        {
            Register(typeof(T), service, key);
        }

        public void Register(Type t, object service)
        {
            Register(t, service, null);
        }

        public void Register(Type t, object service, string key)
        {
            _services[new ServiceKey(t, key)] = service;
        }


        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            Contract.RequiresNotNull(serviceType, nameof(serviceType));

            return _services
                .Where(kvp => kvp.Key.Type.Equals(serviceType))
                .Select(kvp => kvp.Value);
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            Contract.RequiresNotNull(serviceType, nameof(serviceType));

            if (_services.TryGetValue(new ServiceKey(serviceType, key), out object svc))
            {
                return svc;
            }

            return null;
        }


        private readonly struct ServiceKey : IEquatable<ServiceKey>
        {
            public Type Type { get; }
            public string Key { get; }


            public ServiceKey(Type type, string key)
            {
                Type = type;
                Key = key;
            }


            public bool Equals(ServiceKey other)
            {
                return Type.Equals(other.Type)
                    && string.CompareOrdinal(Key, other.Key) == 0;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 1195268993;
                    hashCode = hashCode * -1521134295 + Type.GetHashCode();
                    hashCode = hashCode * -1521134295 + (Key?.GetHashCode() ?? 0);
                    return hashCode;
                }
            }
        }
    }
}
