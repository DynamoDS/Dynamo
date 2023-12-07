using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Windows.Threading;
using System.Threading;
using NUnit.Framework;

namespace DynamoCoreWpfTests.Utility
{
    public static class DispatcherUtil
    {
        /// <summary>
        ///     Force the Dispatcher to empty it's queue
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// Force the Dispatcher to empty it's queue every 100 ms for a maximum 8 seconds or until
        /// the check function returns true.
        /// </summary>
        /// <param name="check">When check returns true, the even loop is stopped.</param>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEventsLoop(Func<bool> check = null)
        {
            const int max_count = 80;

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
