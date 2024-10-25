using System;
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

            // Invoke with the lowest priority possible so that other tasks (with higher priority) can get a chance to finish.
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.SystemIdle,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// Force the Dispatcher to empty it's queue every 100 ms for a maximum 20 seconds or until
        /// the check function returns true.
        /// </summary>
        /// <param name="check">When check returns true, the even loop is stopped.</param>
        public static void DoEventsLoop(Func<bool> check = null)
        {
            const int max_count = 200;

            int count = 0;
            while (true)
            {
                if (check != null && check())
                {
                    return;
                }
                if (count >= max_count)
                {
                    return;
                }

                DispatcherUtil.DoEvents();
                Thread.Sleep(100);
                count++;
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
