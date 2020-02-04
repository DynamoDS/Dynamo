using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// The DocumentationBrowser view extension displays web or local html files on the Dynamo right panel.
    /// It reacts to documentation display request events in Dynamo to know what and when to display documentation.
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
        public string Name => "Documentation Browser";

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId => "68B45FC0-0BD1-435C-BF28-B97CB03C71C8";

        /// <summary>
        /// Setting to allow or disallow the documentation browser to load remote resources
        /// such as web addresses or files from network shares. Defaults to false.
        /// </summary>
        public bool AllowRemoteResources { get; set; }

        public DocumentationBrowserViewExtension()
        {
            // defaults to false for security considerations
            // mechanisms to expose a setting in Dynamo or in the menu could be added 
            // and the extension could respect those settings by setting this property
            this.AllowRemoteResources = false;
        }

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
            ViewModel = new DocumentationBrowserViewModel()
            {
                AllowRemoteResources = this.AllowRemoteResources
            };
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
            this.Dispose();
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            viewLoadedParams.RequestOpenDocumentationLink -= HandleRequestOpenDocumentationLink;

            BrowserView?.Dispose();
            ViewModel?.Dispose();
        }

        #endregion

        /// <summary>
        /// This method handles the documentation open requests coming from Dynamo.
        /// The incoming request is routed to the ViewModel for processing.
        /// </summary>
        /// <param name="args">The incoming event data.</param>
        public void HandleRequestOpenDocumentationLink(OpenDocumentationLinkEventArgs args)
        {
            if (args is null) return;

            // if disallowed, ignore events targeting remote resources so the sidebar is not displayed
            if (args.IsRemoteResource && !this.AllowRemoteResources) 
                return;

            // make sure the view is added to the Sidebar
            // this also forces the Sidebar to open
            AddToSidebar();

            // forward the event to the ViewModel to handle
            this.ViewModel.HandleOpenDocumentationLinkEvent(args);
        }

        private void AddToSidebar()
        {
            // verify the browser window has been initialised
            if (this.BrowserView is null)
            {
                this.OnMessageLogged(LogMessage.Error(Resources.BrowserViewCannotBeAddedToSidebar));
                return;
            }

            // make sure the documentation window is not empty before displaying it
            // we have to do this here because we cannot detect when the sidebar is displayed
            this.ViewModel.EnsurePageHasContent();

            this.viewLoadedParams?.AddToExtensionsSideBar(this, BrowserView);
        }
    }
}
