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

            container.Bind<IA, A>(singleInstance: true);
            container.Bind<IB, B>();
            //container.Bind<IC, C>();

            var c = container.Resolve<C>();
            var c2 = container.Resolve<C>();

            Console.WriteLine($"c == c2? {ReferenceEquals(c, c2)}");
            Console.WriteLine($"c.A == c2.A? {ReferenceEquals(c.A, c2.A)}");
            Console.WriteLine($"c.B == c2.B? {ReferenceEquals(c.B, c2.B)}");
        }
    }
}
