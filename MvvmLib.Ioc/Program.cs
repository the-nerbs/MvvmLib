using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Ioc
{
    interface IA { }
    interface IB { }
    interface IC
    {
        IA A { get; }
        IB B { get; }
    }

    class A : IA { }
    class B : IB { }
    class C : IC
    {
        public IA A { get; }
        public IB B { get; }

        public C(IA a, IB b)
        {
            A = a;
            B = b;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var container = new IocContainer();

            container.Bind<IA, A>();
            container.Bind<IB, B>();
            container.Bind<IC, C>();

            var c = container.Resolve<IC>();
        }
    }
}
