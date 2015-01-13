using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    public static class Disposable
    {
        private sealed class SimpleDisposable : IDisposable
        {
            private readonly Action action;

            public SimpleDisposable(Action dispose)
            {
                action = dispose;
            }

            public void Dispose()
            {
                action();
            }
        }

        public static IDisposable Create(Action disposeAction)
        {
            return new SimpleDisposable(disposeAction);
        }
    }
}
