using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Interaction logic for DocumentationBrowserView.xaml
    /// </summary>
    public partial class DocumentationBrowserView : UserControl, IDisposable
    {
        readonly DocumentationBrowserViewModel viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public DocumentationBrowserView(DocumentationBrowserViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;
            viewModel.LinkChanged += this.NavigateToPage;
            documentationBrowser.AllowDrop = false;
            documentationBrowser.NavigationStarting += DocumentationBrowser_NavigationStarting;
        }

        /// <summary>
        /// Redirect the user to the browser if they press a link in the documentation browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentationBrowser_NavigationStarting(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationStartingEventArgs e)
        {
            // we need to do a null check as sometimes this event will fire twice and return null
            if (e.Uri == null) return;
            // check if the argument Uri is the same as the warning link
            if (e.Uri.Equals(viewModel.Link)) return;
            // for local files the argument Uri will return 'about:blank'
            if (e.Uri.OriginalString.Equals("about:blank")) return;

            // if non of the above is true cancel the navigation 
            // and redirect it to a new process that starts the browser
            e.Cancel = true;
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        public void NavigateToPage(Uri link)
        {
            if (this.viewModel.IsRemoteResource)
            {
                RemoteLinkBanner.Visibility = Visibility.Visible;
                documentationBrowser.Navigate(link);
                return;
            }

            RemoteLinkBanner.Visibility = Visibility.Collapsed;
            documentationBrowser.NavigateToString(this.viewModel.Content);
        }

        /// <summary>
        /// Dispose function for DocumentationBrowser
        /// </summary>
        public void Dispose()
        {
            this.viewModel.LinkChanged -= this.NavigateToPage;
            documentationBrowser.Dispose();
        }
    }
}
