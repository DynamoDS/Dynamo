using System.Windows;
using Dynamo.Wpf.Properties;
using DynamoUtilities;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.Wpf.Utilities
{
    public class DynamoWebView2 : WebView2
    {
        #region API/Data used for debugging/testing
        private string stamp;
        #endregion

        public DynamoWebView2() : base()
        {
            stamp = TestUtilities.WebView2Stamp;
        }

        protected override void Dispose(bool disposing)
        {
            if (System.Environment.CurrentManagedThreadId != Dispatcher.Thread.ManagedThreadId)
            {
                System.Console.WriteLine($"WebView2 instance with stamp {stamp} is being disposed of on non-UI thread");
            }
            if (Dispatcher != null)
            {
                Dispatcher.Invoke(() =>
                {
                    base.Dispose(disposing);
                });
            }
            else
            {
                System.Console.WriteLine($"WebView2 instance with stamp {stamp} is being disposed of but has no valid Dispatcher");
                // Should we still try to dispose ?
                base.Dispose(disposing);
            }
        }
    }

    /// <summary>
    /// This class will contain several utility functions that will be used for the WebView2 component
    /// </summary>
    public static class WebView2Utilities
    {
        /// <summary>
        /// Validate if the WebView2 Evergreen Runtime is installed in the computer, otherwise it will show a MessageBox about installing the Runtime and then exit Dynamo
        /// </summary>
        /// <returns></returns>
        public static bool ValidateWebView2RuntimeInstalled()
        {
            try
            {
                string availableVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
                return true;
            }
            //We reach the catch section only when the Webview2 Runtime is not installed
            catch (WebView2RuntimeNotFoundException)
            {
                var messageStr = Resources.ResourceManager.GetString("WebView2RequiredMessage");
                if (messageStr.IndexOf("\\n") >= 0)
                    messageStr = messageStr.Replace("\\n", "\n");

                MessageBoxService.Show(messageStr,
                                       Resources.WebView2RequiredTitle,
                                       true,
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Error);
                return false;
            }
        }
    }
}
