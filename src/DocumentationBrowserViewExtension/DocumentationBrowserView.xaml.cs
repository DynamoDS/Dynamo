using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Interaction logic for DocumentationBrowserView.xaml
    /// </summary>
    public partial class DocumentationBrowserView : UserControl, IDisposable
    {
        private const string LOCAL_FILES_URI_IN_BROWSER = "about:blank";
        private readonly DocumentationBrowserViewModel viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public DocumentationBrowserView(DocumentationBrowserViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;

            // subscribe to the link changed event on the view model
            // so we know when to navigate to a new documentation page/document
            viewModel.LinkChanged += NavigateToPage;

            // handle brower component events & disable certain features that are not needed
            this.documentationBrowser.AllowDrop = false;
            this.documentationBrowser.NavigationStarting += NavigationStarting;
            this.documentationBrowser.NavigationCompleted += NavigationCompleted;
        }

        private void NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            // do something here ?
        }

        /// <summary>
        /// Redirect the user to the browser if they press a link in the documentation browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationStarting(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationStartingEventArgs e)
        {
            // we need to do a null check as sometimes this event will fire twice and return null
            if (e.Uri == null) return;
            // check if the argument Uri is the same as the warning link
            if (e.Uri.Equals(this.viewModel.Link)) return;
            // for local files the argument Uri will return 'about:blank'
            if (e.Uri.OriginalString.Equals(LOCAL_FILES_URI_IN_BROWSER)) return;

            // if non of the above is true cancel the navigation 
            // and redirect it to a new process that starts the browser
            e.Cancel = true;
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        public void NavigateToPage(Uri link)
        {
            if (this.viewModel.IsRemoteResource)
            {
                this.RemoteLinkBanner.Visibility = Visibility.Visible;
                this.documentationBrowser.Navigate(link);
                return;
            }

            this.RemoteLinkBanner.Visibility = Visibility.Collapsed;
            this.documentationBrowser.NavigateToString(this.viewModel.GetContent());
        }

        /// <summary>
        /// Dispose function for DocumentationBrowser
        /// </summary>
        public void Dispose()
        {
            this.viewModel.LinkChanged -= NavigateToPage;
            this.documentationBrowser.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
