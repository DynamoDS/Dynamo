using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Dynamo.Graphics
{
    public class VisualizerHwndHost : HwndHost, IDisposable
    {
        internal const int WS_CHILD = 0x40000000;
        internal const int WS_VISIBLE = 0x10000000;
        internal const int LBS_NOTIFY = 0x00000001;
        internal const int HOST_ID = 0x00000002;
        internal const int LISTBOX_ID = 0x00000001;
        internal const int WS_VSCROLL = 0x00200000;
        internal const int WS_BORDER = 0x00800000;

        private IntPtr hwndVisualizer = IntPtr.Zero;
        private System.Windows.Size dimension;

        public VisualizerHwndHost(double width, double height)
        {
            dimension = new System.Windows.Size(width, height);
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
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var width = ((int)dimension.Width);
            var height = ((int)dimension.Height);

            hwndVisualizer = CreateWindowEx(0, "static", "", WS_CHILD | WS_VISIBLE,
                0, 0, width, height, hwndParent.Handle, (IntPtr)HOST_ID, 
                IntPtr.Zero, IntPtr.Zero);

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
        }

        #region PInvoke Class Static Methods

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern System.UInt16 RegisterClassW(
            [In] ref WNDCLASS lpWndClass
        );

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        static extern IntPtr CreateWindowEx(
            UInt32 dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpWindowName,
            UInt32 dwStyle,
            Int32 x,
            Int32 y,
            Int32 nWidth,
            Int32 nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        [DllImport("user32.dll", SetLastError = true)]
        static extern System.IntPtr DefWindowProcW(
            IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam
        );

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(
            IntPtr hwnd
        );

        #endregion
    }
}
