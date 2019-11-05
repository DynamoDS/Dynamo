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
    public partial class DocumentationBrowserView : Window, IDisposable
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
        }

        public void NavigateToPage(Uri link)
        {
            if (this.viewModel.IsRemoteResource) documentationBrowser.Navigate(link);
            else documentationBrowser.NavigateToString(this.viewModel.Content);
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
