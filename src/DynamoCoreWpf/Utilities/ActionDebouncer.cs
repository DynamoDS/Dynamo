using Dynamo.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// The ActionDebouncer class offers a means to redunce the number of UI notifications for a specified time.
    /// It is meant to be used in UI elements where too many UI updates can cause perfomance issues.
    /// </summary>
    internal class ActionDebouncer(ILogger logger) : IDisposable
    {
        private readonly ILogger logger = logger;
        private CancellationTokenSource cts;

        public void Cancel()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        /// <summary>
        /// Delays the "action" for a "timeout" number of milliseconds
        /// The input Action will run on same syncronization context as the Debounce method call.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait</param>
        /// <param name="action">The action to execute after the timeout runs out.</param>
        /// <returns>A task that finishes when the deboucing is cancelled or the input action has completed (successfully or not). Should be discarded in most scenarios.</returns>
        public async Task Debounce(int timeout, Action action)
        {
            Cancel();
            cts = new CancellationTokenSource();
            try
            {
                await Task.Delay(timeout, cts.Token);
                action();
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                logger?.Log("Failed to run debounce action with the following error:");
                logger?.Log(ex.ToString());
            }
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
