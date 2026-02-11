using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Models;
using Dynamo.Wpf.Properties;
using DynamoUtilities;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Custom Webview2 class designed to have a safer cleanup logic and give better debugging capabilities   
    /// </summary>
    public class DynamoWebView2 : WebView2
    {
        #region API/Data used for debugging/testing
        private string tag;
        #endregion

        Action<string> logger = System.Console.WriteLine;
        private Task initTask = null;
        private bool disposeCalled;

        public DynamoWebView2() : base()
        {
            tag = TestUtilities.WebView2Tag;
            DefaultBackgroundColor = System.Drawing.Color.FromArgb(0, 0, 0, 0); // Transparent background to prevent flickering on load
        }

        /// <summary>
        /// Wrapper over WebView2's EnsureCoreWebView2Async
        /// Use this method instead of EnsureCoreWebView2Async
        /// </summary>
        /// <param name="logFn">Optional logging function. Will default to System.Console</param>
        /// <returns></returns>
        internal async Task Initialize(Action<string> logFn = null)
        {
            logger = logFn ?? logger;
            initTask = EnsureCoreWebView2Async();
            await initTask;

            ObjectDisposedException.ThrowIf(disposeCalled, this);
        }

        /// <summary>
        /// Configures standard security and usability settings for WebView2 instances.
        /// This method applies recommended settings to standardize WebView2 behavior across the application.
        /// </summary>
        /// <param name="enableZoomControl">Enable zoom controls (default: false)</param>
        /// <param name="enableDevTools">Enable developer tools for debugging (default: false)</param>
        /// <param name="enableContextMenu">Enable right-click context menu (default: false)</param>
        public void ConfigureSettings(bool enableZoomControl = false, bool enableDevTools = false, bool enableContextMenu = false)
        {
            if (CoreWebView2 == null)
            {
                throw new InvalidOperationException("CoreWebView2 is not initialized. Call Initialize() before ConfigureSettings().");
            }

            var settings = CoreWebView2.Settings;

            // Security: Disable browser accelerator keys (Ctrl+P, Ctrl+F, F5, F12, etc.)
            settings.AreBrowserAcceleratorKeysEnabled = false;

            // Security: Control context menu access (prevents "View Source", "Inspect", etc.)
            settings.AreDefaultContextMenusEnabled = enableContextMenu;

            // UI: Disable status bar showing URLs on hover
            settings.IsStatusBarEnabled = false;

            // UI: Control zoom capabilities
            settings.IsZoomControlEnabled = enableZoomControl;
            settings.IsPinchZoomEnabled = false;

            // Development: Control DevTools access
            settings.AreDevToolsEnabled = enableDevTools;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposeCalled) return;
            disposeCalled = true;

            if (System.Environment.CurrentManagedThreadId != Dispatcher.Thread.ManagedThreadId)
            {
                logger?.Invoke($"WebView2 instance with tag {tag} is being disposed of on non-UI thread");
            }

            if (initTask != null && !initTask.IsCompleted)
            {
                logger?.Invoke($"WebView2 instance with tag {tag} is being disposed but async initialization is still not done");

                // Wait for EnsureCoreWebView2Async to finish before we continue with dispose.
                // This way we avoid EnsureCoreWebView2Async resuming execution while webview2 is disposed.
                initTask.ContinueWith((t) => {
                    // This continuation runs even if initTask has been cancelled
                    Dispatcher.Invoke(() => base.Dispose(disposing));
                });
            }
            else
            {
                Dispatcher.Invoke(() => base.Dispose(disposing));
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

        /// <summary>
        /// Returns the user data folder path for WebView2 (used in SplashScreen, HomePage, PackageManagerWizard)
        /// </summary>
        /// <returns>user data folder path for WebView2</returns>
        internal static string GetTempDirectory()
        {
            // Create a temp folder unique to this Dynamo instance based on process id
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string tmpDataFolder = Path.Combine(
                localAppData,
                "Temp",
                Configurations.DynamoAsString,
                "WebView2");
            return tmpDataFolder;
        }
    }
}
