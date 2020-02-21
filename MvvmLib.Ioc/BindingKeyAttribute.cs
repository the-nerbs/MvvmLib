using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Ioc
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class BindingKeyAttribute : Attribute
    {
        public string Key { get; }
        public bool FallbackToDefaultBinding { get; }


        public BindingKeyAttribute(string key)
            : this(key, false)
        { }

        public BindingKeyAttribute(string key, bool fallbackToDefaultBinding)
        {
            Key = key;
            FallbackToDefaultBinding = fallbackToDefaultBinding;
        }
    }
}
