using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Dynamorph
{
    class VisualizerHwndHost : HwndHost, IDisposable
    {
        // internal VisualizerHwndHost()
        // {
        //     this.MessageHook += OnHwndHostMessage;
        // }
        // 
        // IntPtr OnHwndHostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        // {
        // }

        protected override void Dispose(bool disposing)
        {
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var hwndVisualizer = Visualizer.Create(hwndParent.Handle, 320, 240);
            return new HandleRef(this, hwndVisualizer);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg,
            IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return System.IntPtr.Zero;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        #region PInvoke Class Static Methods

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        #endregion
    }
}
