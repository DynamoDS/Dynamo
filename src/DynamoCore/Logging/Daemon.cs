using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Dynamo.Logging
{
    internal class Daemon : IDisposable
    {
        private Thread daemonThread;
        private CancellationTokenSource cts;
        private KeyboardInput keyboard;
        //private ManualResetEvent mre;
        private bool eventFired;

        public Daemon(CancellationTokenSource cts)
        {
            this.cts = cts;
            //keyboard = new KeyboardInput();
            //keyboard.KeyBoardKeyPressed += keyboard_KeyBoardKeyPressed;
            daemonThread = new Thread(ThreadProc);
            daemonThread.IsBackground = true;
            daemonThread.Start();
        }

        void ThreadProc()
        {
            //keyboard = new KeyboardInput();
            //keyboard.KeyBoardKeyPressed += keyboard_KeyBoardKeyPressed;
            while (WindowsHookHelper.GetAsyncKeyState(0x1B) == 0)
            {
                Thread.Sleep(100);
            }
            cts.Cancel();
            //keyboard.KeyBoardKeyPressed -= keyboard_KeyBoardKeyPressed;
            //keyboard.Dispose();
        }

        //void keyboard_KeyBoardKeyPressed(object sender, EventArgs e)
        //{
        //    short key = WindowsHookHelper.GetAsyncKeyState(0x1B);
        //    cts.Cancel();
        //    eventFired = true;
        //}

        public void Dispose()
        {
            //keyboard.KeyBoardKeyPressed -= keyboard_KeyBoardKeyPressed;
            //keyboard.Dispose();
        }
    }

    public class WindowsHookHelper
    {
        public delegate IntPtr HookDelegate(
            Int32 Code, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hHook, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern IntPtr UnhookWindowsHookEx(IntPtr hHook);


        [DllImport("User32.dll")]
        public static extern IntPtr SetWindowsHookEx(
            Int32 idHook, HookDelegate lpfn, IntPtr hmod,
            Int32 dwThreadId);

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Int32 vKey);
    }

    public class KeyboardInput : IDisposable
    {
        public event EventHandler<EventArgs> KeyBoardKeyPressed;

        private WindowsHookHelper.HookDelegate keyBoardDelegate;
        private IntPtr keyBoardHandle;
        private const Int32 WH_KEYBOARD_LL = 13;
        private bool disposed;

        public KeyboardInput()
        {
            keyBoardDelegate = KeyboardHookDelegate;
            keyBoardHandle = WindowsHookHelper.SetWindowsHookEx(
                WH_KEYBOARD_LL, keyBoardDelegate, IntPtr.Zero, 0);
        }

        private IntPtr KeyboardHookDelegate(
            Int32 Code, IntPtr wParam, IntPtr lParam)
        {
            if (Code < 0)
            {
                return WindowsHookHelper.CallNextHookEx(
                    keyBoardHandle, Code, wParam, lParam);
            }

            if (KeyBoardKeyPressed != null)
                KeyBoardKeyPressed(this, new EventArgs());

            return WindowsHookHelper.CallNextHookEx(
                keyBoardHandle, Code, wParam, lParam);
        }

        

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (keyBoardHandle != IntPtr.Zero)
                {
                    WindowsHookHelper.UnhookWindowsHookEx(
                        keyBoardHandle);
                }

                disposed = true;
            }
        }

        ~KeyboardInput()
        {
            Dispose(false);
        }
    }
}
