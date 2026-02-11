using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DynamoCoreWpfTests.Utility
{
    public static class DispatcherUtil
    {
        /// <summary>
        ///     Force the Dispatcher to empty it's queue
        /// </summary>
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();

            // Invoke with the lowest priority possible so that other tasks (with higher priority) can get a chance to finish.
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.SystemIdle,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// Force the Dispatcher to empty it's queue every 100 ms for a maximum of timeoutSeconds seconds or until
        /// the check function returns true.
        /// </summary>
        /// <param name="check">When check returns true, the even loop is stopped.</param>
        public static void DoEventsLoop(Func<bool> check = null, int timeoutSeconds = 60)
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            Task.Delay(timeoutSeconds * 1000).ContinueWith(t =>
            {
                cts.Cancel();
                cts.Dispose();
            });

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (check != null && check())
                {
                    return;
                }

                DoEvents();
                Thread.Sleep(100);
            }
        }

    /// <summary>
    ///     Helper method for DispatcherUtil
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
}
