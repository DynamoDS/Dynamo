using System;
using System.Diagnostics;
using System.Threading;
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

            // Use Background priority so the exit callback is not starved when
            // WebView2 or other components continuously post higher-priority messages.
            // The previous SystemIdle priority could be starved indefinitely, causing
            // PushFrame to block forever and making the DoEventsLoop timeout ineffective.
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background,
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
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed.TotalSeconds < timeoutSeconds)
            {
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
