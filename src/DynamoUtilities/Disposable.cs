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
            private readonly Action dispose;

            internal SimpleDisposable(Action dAction)
            {
                dispose = dAction;
            }

            internal SimpleDisposable(Action cAction, Action dAction)
            {
                cAction();

                dispose = dAction;
            }

            public void Dispose()
            {
                dispose();
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

        /// <summary>
        /// Constructs a new disposable that calls the constructorAction when constructed and the disposeAction when disposed 
        /// </summary>
        /// <param name="constructorAction">An action that runs when this object is constructed</param>
        /// <param name="disposeAction">An action that runs when this object is disposed</param>
        /// <returns>New disposable object</returns>
        internal static IDisposable Create(Action constructorAction, Action disposeAction)
        {
            return new SimpleDisposable(constructorAction, disposeAction);
        }

    }
}
