using Dynamo.Controls;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// The DocumentationBrowser view extension displays web or local html files on the Dynamo right panel.
    /// It reacts to documentation display request events in Dynamo to know what and when to display documentation.
    /// </summary>
    public class DocumentationBrowserViewExtension : IViewExtension, ILogSource
    {
        private ViewLoadedParams viewLoadedParamsReference;
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

        public DocumentationBrowserViewExtension()
        {
            // initialise the ViewModel and View for the window
            this.ViewModel = new DocumentationBrowserViewModel();
            this.BrowserView = new DocumentationBrowserView(this.ViewModel);
        }

        #region ILogSource

        public event Action<ILogMessage> MessageLogged;
        internal void OnMessageLogged(ILogMessage msg)
        {
            MessageLogged?.Invoke(msg);
        }
        #endregion

        #region IViewExtension lifecycle

        public void Startup(ViewStartupParams viewLoadedParams)
        {
            // Do nothing for now
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (viewLoadedParams == null) throw new ArgumentNullException(nameof(viewLoadedParams));

            this.ViewModel.MessageLogged += OnViewModelMessageLogged;

            this.viewLoadedParamsReference = viewLoadedParams; 

            // Add a button to Dynamo View menu to manually show the window
            this.documentationBrowserMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
            this.documentationBrowserMenuItem.Checked += (sender, args) =>
            {
                AddToSidebar(true);
            };
            this.documentationBrowserMenuItem.Unchecked += (sender, args) =>
            {
                viewLoadedParams.CloseExtensioninInSideBar(this);
            };
            this.viewLoadedParamsReference.AddMenuItem(MenuBarType.View, this.documentationBrowserMenuItem);

            DynamoView.CloseExtension += OnCloseExtension;

            // subscribe to the documentation open request event from Dynamo
            this.viewLoadedParamsReference.RequestOpenDocumentationLink += HandleRequestOpenDocumentationLink;

            // subscribe to property changes of DynamoViewModel so we can show/hide the browser on StartPage display
            (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged += HandleStartPageVisibilityChange;
        }

        public void Shutdown()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            DynamoView.CloseExtension -= OnCloseExtension;
            this.viewLoadedParamsReference.RequestOpenDocumentationLink -= HandleRequestOpenDocumentationLink;
            this.ViewModel.MessageLogged -= OnViewModelMessageLogged;

            this.BrowserView?.Dispose();
            this.ViewModel?.Dispose();
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// This method handles the documentation open requests coming from Dynamo.
        /// The incoming request is routed to the ViewModel for processing.
        /// </summary>
        /// <param name="args">The incoming event data.</param>
        public void HandleRequestOpenDocumentationLink(OpenDocumentationLinkEventArgs args)
        {
            if (args == null) return;

            // ignore events targeting remote resources so the sidebar is not displayed
            if (args.IsRemoteResource)
                return;

            // make sure the view is added to the Sidebar
            // this also forces the Sidebar to open
            AddToSidebar(false);

            // forward the event to the ViewModel to handle
            this.ViewModel?.HandleOpenDocumentationLinkEvent(args);
        }

        private void OnCloseExtension(String extensionTabName)
        {
            if (extensionTabName.Equals(Name))
            {
                this.documentationBrowserMenuItem.IsChecked = false;
            }
        }

        private void OnViewModelMessageLogged(ILogMessage msg)
        {
            OnMessageLogged(msg);
        }

        private void AddToSidebar(bool displayDefaultContent)
        {
            // verify the browser window has been initialized
            if (this.BrowserView == null)
            {
                OnMessageLogged(LogMessage.Error(Resources.BrowserViewCannotBeAddedToSidebar));
                return;
            }

            // make sure the documentation window is not empty before displaying it
            // we have to do this here because we cannot detect when the sidebar is displayed
            if (displayDefaultContent)
            {
                this.ViewModel?.EnsurePageHasContent();
            }

            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.BrowserView);
        }

        // hide browser directly when startpage is shown to deal with air space problem.
        // https://github.com/dotnet/wpf/issues/152
        private void HandleStartPageVisibilityChange(object sender, PropertyChangedEventArgs e)
        {
            DynamoViewModel dynamoViewModel = sender as DynamoViewModel;

            if (dynamoViewModel != null && e.PropertyName == nameof(DynamoViewModel.ShowStartPage))
            {
                ViewModel.ShowBrowser = !dynamoViewModel.ShowStartPage;
            }
        }
    }
}
