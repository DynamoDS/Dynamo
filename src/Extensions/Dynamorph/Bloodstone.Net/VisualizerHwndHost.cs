using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Bloodstone
{
    class VisualizerHwndHost : HwndHost, IDisposable
    {
        private System.Windows.Size dimension;

        internal VisualizerHwndHost(double width, double height)
        {
            dimension = new System.Windows.Size(width, height);
        }

        internal Visualizer CurrentVisualizer
        {
            get { return Visualizer.CurrentInstance(); }
        }

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
            Visualizer.Destroy();
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var width = ((int)dimension.Width);
            var height = ((int)dimension.Height);
            var hwndVisualizer = Visualizer.Create(hwndParent.Handle, width, height);
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
            Visualizer.Destroy();
        }

        #region PInvoke Class Static Methods

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        #endregion
    }
}
