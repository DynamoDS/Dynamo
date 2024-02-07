

using DynamoCoreWpfTests.Utility;
using DynamoUtilities;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
        public void SplashScreen_CloseExplicitPropIsCorrect1()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.RequestLaunchDynamo(true);
            Assert.IsFalse(ss.CloseWasExplicit);

            ss.CloseWindow();
        }

        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect2()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            Assert.IsFalse(ss.CloseWasExplicit);

            ss.CloseWindow();
        }

        [Test]
        public void SplashScreen_CloseExplicitPropIsCorrect3()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();
            ss.CloseWindow();
            Assert.IsTrue(ss.CloseWasExplicit);
        }

        [Test]
        public void SplashScreen_MultipleCloseMessages()
        {
            var ss = new Dynamo.UI.Views.SplashScreen();

            void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
            {
                ss.webView.NavigationCompleted -= WebView_NavigationCompleted;

                IntPtr WindowToFind = FindWindow(null, "Dynamo SplashScreen");
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

            bool windoClosed = false;
            void WindowClosed(object sender, EventArgs e)
            {
                windoClosed = true;
                ss.Closed -= WindowClosed;
            }

            ss.Closed += WindowClosed;

            ss.Show();

            DispatcherUtil.DoEventsLoop(() => windoClosed);

            Assert.IsNull(ss.webView);// Make sure webview2 was disposed
            Assert.IsTrue(windoClosed);// Make sure the window was closed
        }
    }
}
