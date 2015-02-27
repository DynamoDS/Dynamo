using System;

namespace Dynamo.Utilities
{
    public class OnceDisposable : IDisposable
    {
        private bool called;
        private IDisposable disposable;

        public OnceDisposable(IDisposable disposable)
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