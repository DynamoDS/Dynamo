using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    public partial class DocumentationBrowserView : Window, IDisposable
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public DocumentationBrowserView(DocumentationBrowserViewExtension viewExtension, ViewLoadedParams p)
        {
            InitializeComponent();
            DocumentationBrowserViewModel.LinkChanged += this.NavigateToPage;
            documentationBrowser.NavigationStarting += DocumentationBrowser_NavigationStarting;
        }

        private void DocumentationBrowser_NavigationStarting(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationStartingEventArgs e)
        {
            if (e.Uri == null) return;
            if (e.Uri.Scheme != Uri.UriSchemeHttp && e.Uri.Scheme != Uri.UriSchemeHttps) return;

            e.Cancel = true;
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));       
        }

        public void NavigateToPage(string link)
        {
            documentationBrowser.NavigateToString(link);
        }

        /// <summary>
        /// Dispose function for DocumentationBrowser
        /// </summary>
        public void Dispose()
        {
            DocumentationBrowserViewModel.LinkChanged -= this.NavigateToPage;
            documentationBrowser.Dispose();
        }
    }
}
