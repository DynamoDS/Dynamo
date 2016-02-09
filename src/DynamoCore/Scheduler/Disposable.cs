using System;

namespace Dynamo.Scheduler
{
    public static class Disposable
    {
        private sealed class SimpleDisposable : IDisposable
        {
            private readonly Action action;

            internal SimpleDisposable(Action dispose)
            {
                action = dispose;
            }

            public void Dispose()
            {
                action();
            }
        }

        internal static IDisposable Create(Action disposeAction)
        {
            return new SimpleDisposable(disposeAction);
        }
    }
}
