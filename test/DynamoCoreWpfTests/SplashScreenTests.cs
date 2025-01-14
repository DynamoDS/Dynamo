using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DynamoUtilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{

    [TestFixture]
    internal class SplashScreenTests
    {
        public enum WindowsMessage
        {
            WM_CLOSE = 0x0010
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, String lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string lpString);


        [SetUp]
        public void SetUp()
        {
            TestUtilities.WebView2Tag = TestContext.CurrentContext.Test.Name;
        }

        [TearDown]
        public void CleanUp()
        {
            TestUtilities.WebView2Tag = string.Empty;
        }
      
        [Test]
        //note that this test sends a windows close message directly to the window
        //but skips the JS interop that users rely on to close the window - so that is not tested by this test.
        public void SplashScreen_MultipleCloseMessages()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.Title = "Dynamo SplashScreen Test";

            void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
            {
                ss.webView.NavigationCompleted -= WebView_NavigationCompleted;

                IntPtr WindowToFind = FindWindow(null, "Dynamo SplashScreen Test");
                Debug.Assert(WindowToFind != IntPtr.Zero);

                // Simulate clicking on the close button several times while the main thread is stuck waiting.
                _ = Task.Run(() =>
                {
                    Thread.Sleep(100);
                    _ = SendMessage(WindowToFind, (int)WindowsMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                });
                _ = Task.Run(() =>
                {
                    Thread.Sleep(100);
                    _ = SendMessage(WindowToFind, (int)WindowsMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                });

                Task.Delay(1000).Wait();

            }
            ss.webView.NavigationCompleted += WebView_NavigationCompleted;

            bool windowClosed = false;
            void WindowClosed(object sender, EventArgs e)
            {
                windowClosed = true;
            }

            ss.Closed += WindowClosed;

            ss.Show();

            DispatcherUtil.DoEventsLoop(() => windowClosed, 50);

            ss.Closed -= WindowClosed;

            Assert.IsNull(ss.webView);// Make sure webview2 was disposed
            Assert.IsTrue(windowClosed);// Make sure the window was closed
        }
    }
}
