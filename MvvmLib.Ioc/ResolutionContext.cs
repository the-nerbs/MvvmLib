using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmLib.Ioc
{
    sealed class ResolutionContext
    {
        private readonly Stack<RegistrationKey> _activationStack = new Stack<RegistrationKey>();


        public ActivationScope Activating(RegistrationKey regKey)
        {
            return new ActivationScope(this, regKey);
        }

        public bool WouldCreateCycle(RegistrationKey reg)
        {
            return _activationStack.Contains(reg);
        }


        public readonly struct ActivationScope : IDisposable
        {
            private readonly ResolutionContext _context;

            public ActivationScope(ResolutionContext context, RegistrationKey regKey)
            {
                _context = context;
                _context._activationStack.Push(regKey);
            }

            public void Dispose()
            {
                _context._activationStack.Pop();
            }
        }
    }
}
