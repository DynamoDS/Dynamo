using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Windows.Controls;

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
        public string UniqueId => "68B45FC0-0BD1-435C-BF28-B97CB03C71C8";

        #region ILogSource

        public event Action<ILogMessage> MessageLogged;
        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }
        #endregion

        #region IViewExtension lifecycle

        public void Startup(ViewStartupParams viewLoadedParams)
        {
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.viewLoadedParams = viewLoadedParams ?? throw new ArgumentNullException(nameof(viewLoadedParams));

            // initialise the ViewModel and View for the window
            ViewModel = new DocumentationBrowserViewModel();
            BrowserView = new DocumentationBrowserView(ViewModel);

            // Add a button to Dynamo View menu to manually show the window
            documentationBrowserMenuItem = new MenuItem { Header = Resources.MenuItemText };
            documentationBrowserMenuItem.Click += (sender, args) =>
            {
                AddToSidebar();
            };
            this.viewLoadedParams.AddMenuItem(MenuBarType.View, documentationBrowserMenuItem);

            // subscribe to the documentation open request event from Dynamo
            this.viewLoadedParams.RequestOpenDocumentationLink += HandleRequestOpenDocumentationLink;
        }

        public void Shutdown()
        {
            BrowserView.Dispose();
            ViewModel.Dispose();
            this.Dispose();
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            // un-subscribe from the documentation open request event from Dynamo
            viewLoadedParams.RequestOpenDocumentationLink -= HandleRequestOpenDocumentationLink;
        }

        #endregion

        public void HandleRequestOpenDocumentationLink(OpenDocumentationLinkEventArgs args)
        {
            if (args is null) return;

            // make sure the view is added to the Sidebar
            // this also forces the Sidebar to open
            AddToSidebar();

            // forward the event to the browser ViewModel to handle
            this.ViewModel.HandleOpenDocumentationLinkEvent(args);
        }

        private void AddToSidebar()
        {
            if (this.BrowserView is null) this.OnMessageLogged(LogMessage.Error("Browser view was not initialised and cannot be added to the sidebar."));
            this.viewLoadedParams?.AddToExtensionsSideBar(this, BrowserView);
        }
    }
}
