using System;
using System.Linq;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Wpf.Extensions;
using Dynamo.ViewModels;
using System.Windows;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// This sample view extension demonstrates a sample IViewExtension 
    /// which tracks graph dependencies (currently only packages) on the Dynamo right panel.
    /// It reacts to workspace modified/ cleared events to refresh.
    /// </summary>
    public class DocumentationBrowserViewExtension : IViewExtension, ILogSource
    {
        private ViewLoadedParams viewLoadedParams;
        private MenuItem documentationBrowserMenuItem;
        internal DocumentationBrowserView BrowserView { get; private set; }
        internal DocumentationBrowserViewModel ViewModel { get; private set; }

        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name => "Documentation Browser ViewExtension";

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId =>  "011ec935-fcd6-43f0-ab32-1c5c0f913b33";

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            // un-subscribe from the documentation open request event from Dynamo
            viewLoadedParams.RequestOpenDocumentationLink -= HandleOpenDocumentationLinkEvent;
        }


        [Obsolete("This method is not implemented and will be removed.")]
        public void Ready(ReadyParams readyParams)
        {
        }

        public void Shutdown()
        {

            BrowserView.Dispose();
            ViewModel.Dispose();
            this.Dispose();
        }

        public void Startup(ViewStartupParams viewLoadedParams)
        {
        }

        public event Action<ILogMessage> MessageLogged;
        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }

        public void HandleOpenDocumentationLinkEvent(OpenDocumentationLinkEventArgs args)
        {
            if (args is null) return;

            MessageBox.Show("Someone requested to navigate to : " + Environment.NewLine + args.Link.ToString());
            this.ViewModel.IsRemoteResource = args.IsRemoteResource;
            this.ViewModel.OpenDocumentationLink(args.Link.ToString());
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.viewLoadedParams = viewLoadedParams ?? throw new ArgumentNullException(nameof(viewLoadedParams));

            ViewModel = new DocumentationBrowserViewModel();
            BrowserView = new DocumentationBrowserView(this, viewLoadedParams);
            // when a package is loaded update the DependencyView 
            // as we may have installed a missing package.

            // Adding a button in view menu to refresh and show manually
            documentationBrowserMenuItem = new MenuItem { Header = Resources.MenuItemString };
            documentationBrowserMenuItem.Click += (sender, args) =>
            {
                // Refresh dependency data
                this.viewLoadedParams.AddToExtensionsSideBar(this, BrowserView);
                ViewModel.OpenDocumentationLink(string.Empty);
            };
            this.viewLoadedParams.AddMenuItem(MenuBarType.View, documentationBrowserMenuItem);

            // subscribe to the documentation open request event from Dynamo
            this.viewLoadedParams.RequestOpenDocumentationLink += HandleOpenDocumentationLinkEvent;
        }
    }
}
