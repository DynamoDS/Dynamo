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

        /// <summary>
        /// construct a new disposable that calls the delegate when disposed 
        /// </summary>
        /// <param name="disposeAction"> an action that is run when this object is disposed</param>
        /// <returns></returns>
        public static IDisposable Create(Action disposeAction)
        {
            return new SimpleDisposable(disposeAction);
        }
    }
}
