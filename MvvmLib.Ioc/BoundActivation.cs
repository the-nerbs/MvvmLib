using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MvvmLib.Ioc
{
    internal class BoundActivation
    {
        private readonly ConstructorInfo _constructor;
        private readonly BoundActivation[] _parameters;

        private readonly Registration _reg;


        public Type Type
        {
            get
            {
                return _reg is null
                    ? _constructor.DeclaringType
                    : _reg.ServiceType;
            }
        }


        public BoundActivation(ConstructorInfo ctor, IEnumerable<BoundActivation> parameters)
        {
            Debug.Assert(!(ctor is null));
            Debug.Assert(!(parameters is null));

            _constructor = ctor;
            _parameters = parameters.ToArray();
            _reg = null;
        }

        public BoundActivation(Registration reg)
        {
            Debug.Assert(!(reg is null));

            _constructor = null;
            _parameters = null;
            _reg = reg;
        }


        public bool ContainsCycle()
        {
            return ContainsCycle(this, new Stack<Type>());
        }

        private static bool ContainsCycle(BoundActivation a, Stack<Type> types)
        {
            if (types.Contains(a.Type))
            {
                return true;
            }

            types.Push(a.Type);

            if (!(a._parameters is null))
            {
                for (int i = 0; i < a._parameters.Length; i++)
                {
                    if (ContainsCycle(a._parameters[i], types))
                    {
                        return true;
                    }
                }
            }

            types.Pop();
            return false;
        }

        public object CreateObject(ResolutionContext context)
        {
            if (_reg is null)
            {
                object[] paramValues = new object[_parameters.Length];

                for (int i = 0; i < _parameters.Length; i++)
                {
                    paramValues[i] = _parameters[i].CreateObject(context);
                }

                return _constructor.Invoke(paramValues);
            }

            return _reg.GetValue(context);
        }
    }
}
