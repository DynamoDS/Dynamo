using Dynamo.Logging;
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
            this.documentationBrowser.DpiChanged += DocumentationBrowser_DpiChanged;
        }

        private void DocumentationBrowser_DpiChanged(object sender, DpiChangedEventArgs args)
        {
            try
            {
                // it's possible we're trying to invoke this before the adaptDPI function is
                // injected into the script scope, wrap this in a try catch.
                documentationBrowser.InvokeScript("adaptDPI()");
            }
            catch (Exception e)
            {
                viewModel.MessageLogged?.Invoke(LogMessage.Info($"failed to set DPI,{e.Message}"));
            }
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
            {
                e.Cancel = true;
                return;
            }

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

        /// <summary>
        /// Instruct the embedded web browser to navigate to a given link.
        /// If link is remote resource it is loaded from there.
        /// If link is local resource, it is loaded from ViewModel content.
        /// </summary>
        /// <param name="link"></param>
        public void NavigateToPage(Uri link)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.documentationBrowser.NavigateToString(this.viewModel.GetContent());
            }));
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            this.viewModel.LinkChanged -= NavigateToPage;
            this.documentationBrowser.Navigating -= ShouldAllowNavigation;
            // Note to test writers
            // Disposing the document browser will cause future tests
            // that uses the Browser component to crash
            if (!Models.DynamoModel.IsTestMode)
            {
                this.documentationBrowser.Dispose();
            }
            this.documentationBrowser.DpiChanged -= DocumentationBrowser_DpiChanged;
        }

        /// <summary>
        /// Dispose function for DocumentationBrowser
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
