using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Ioc
{
    internal sealed class SimpleConfiguration<T> : IConfiguration<T>
    {
        public T Value { get; }


        public SimpleConfiguration(T value)
        {
            Value = value;
        }
    }
}
