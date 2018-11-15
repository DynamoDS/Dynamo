using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.LibraryUI.ViewModels;

namespace Dynamo.LibraryUI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView(LibraryViewModel viewModel)
        {
            this.DataContext = viewModel;

            // CEF should already be initiallized if running within Revit
            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings { RemoteDebuggingPort = 8088 };

                // Matching Revit 2020 CefSharp Initialization Settings: 
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;
                CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
                CefSharpSettings.ShutdownOnExit = false;

                Cef.Initialize(settings);
            }
            
            InitializeComponent();

            MainAsync(viewModel.Address, this.Browser);

            this.Browser.MenuHandler = new LibraryViewContextMenuHandler();
        }

        private static async void MainAsync(string address, ChromiumWebBrowser browser)
        {
            // Verify browser is initialized
            await LoadPageAsync(browser);
            // Load library address
            await LoadPageAsync(browser, address);
        }

        public static Task LoadPageAsync(IWebBrowser browser, string address = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler += (sender, args) =>
            {
                // Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            if (!string.IsNullOrEmpty(address))
            {
                browser.Load(address);
            }
            return tcs.Task;
        }

        private class LibraryViewContextMenuHandler : IContextMenuHandler
        {
            public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                model.Clear();
            }

            public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                return false;
            }

            public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
            }

            public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return false;
            }
        }
    }
}