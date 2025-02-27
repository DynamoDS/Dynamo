using Dynamo.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// The ActionDebouncer class offers a means to reduce the number of UI notifications for a specified time.
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
        /// The input Action will run on same syncronization context as the Debounce method call (or the thread pool if a sync context does not exist, ex. in non UI tests).
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait</param>
        /// <param name="action">The action to execute after the timeout runs out.</param>
        /// <returns>A task that finishes when the deboucing is cancelled or the input action has completed (successfully or not). Should be discarded in most scenarios.</returns>
        public void Debounce(int timeout, Action action)
        {
            Cancel();
            cts = new CancellationTokenSource();

            // The TaskScheduler.FromCurrentSynchronizationContext() exists only if there is a valid SyncronizationContex.
            // Calling this method from a non UI thread could have a null SyncronizationContex.Current,
            // so in that case we use the default TaskScheduler which uses the thread pool.
            TaskScheduler taskScheduler = null;
            if (SynchronizationContext.Current != null)
            {// This should always be the case in UI threads.
                taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            else
            {
                // This might happen when running tests in non UI threads.
                // But if we are in a UI thread, then log this as a potential error.
                if (System.Windows.Application.Current?.Dispatcher?.Thread == Thread.CurrentThread)
                {// UI thread.
                    logger?.LogError("The UI thread does not seem to have a SyncronizationContext.");
                }
                taskScheduler = TaskScheduler.Default;
            }

            Task.Delay(timeout, cts.Token).ContinueWith((t) =>
            {
                try
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        action();
                    }
                }
                catch (Exception ex)
                {
                    logger?.Log("Failed to run debounce action with the following error:");
                    logger?.Log(ex.ToString());
                }
            }, taskScheduler);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
