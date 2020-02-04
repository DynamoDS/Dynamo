using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Interaction logic for DocumentationBrowserView.xaml
    /// </summary>
    public partial class DocumentationBrowserView : UserControl, IDisposable
    {
        private const string ABOUT_BLANK_URI = "about:blank";
        private readonly DocumentationBrowserViewModel viewModel;

        /// <summary>
        /// Construct a new DocumentationBrowserView given an appropriate viewmodel.
        /// </summary>
        /// <param name="viewModel">The ViewModel to use as source of events and content.</param>
        public DocumentationBrowserView(DocumentationBrowserViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;

            // subscribe to the link changed event on the view model
            // so we know when to navigate to a new documentation page/document
            viewModel.LinkChanged += NavigateToPage;

            // handle browser component events & disable certain features that are not needed
            this.documentationBrowser.AllowDrop = false;
            this.documentationBrowser.Navigating += ShouldAllowNavigation;
            this.documentationBrowser.Navigated += NavigationCompleted;
        }

        private void NavigationCompleted(object sender, NavigationEventArgs e)
        {
            // do something here ?
        }

        /// <summary>
        /// Redirect the user to the browser if they press a link in the documentation browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShouldAllowNavigation(object sender, NavigatingCancelEventArgs e)
        {
            // do not allow refreshes, back or forward navigation
            if (e.NavigationMode != NavigationMode.New)
                return;

            // we never set the uri if navigating to a local document, so safe to navigate
            if (e.Uri == null)
                return;

            // we want to cancel navigation when a clicked link would navigate 
            // away from the page the ViewModel wants to display
            var isAboutBlankLink = e.Uri.OriginalString.Equals(ABOUT_BLANK_URI);
            var isRemoteLinkFromLocalDocument = !e.Uri.Equals(this.viewModel.Link);

            if (isAboutBlankLink || isRemoteLinkFromLocalDocument)
            {
                // in either of these two cases, cancel the navigation 
                // and redirect it to a new process that starts the default OS browser
                e.Cancel = true;
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }
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
            this.documentationBrowser.Navigating -= ShouldAllowNavigation;
            this.documentationBrowser.Navigated -= NavigationCompleted;
            this.documentationBrowser.Dispose();
        }
    }
}
