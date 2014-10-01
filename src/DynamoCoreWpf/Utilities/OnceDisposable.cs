using System;

namespace Dynamo.Utilities
{
    internal class OnceDisposable : IDisposable
    {
        private bool called;
        private IDisposable disposable;

        internal OnceDisposable(IDisposable disposable)
        {
            this.disposable = disposable;
        }

        public void Dispose()
        {
            if (called) return;
            called = true;
            if (disposable != null) disposable.Dispose();
        }
    }
}