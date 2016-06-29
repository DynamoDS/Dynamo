using System;

namespace Dynamo.Scheduler
{
    /// <summary>
    /// Implements IDisposable functionality.
    /// </summary>
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

        /// <summary>
        /// Constructs a new disposable that calls the delegate when disposed 
        /// </summary>
        /// <param name="disposeAction">An action that runs when this object is disposed</param>
        /// <returns>New disposable object</returns>
        public static IDisposable Create(Action disposeAction)
        {
            return new SimpleDisposable(disposeAction);
        }
    }
}
