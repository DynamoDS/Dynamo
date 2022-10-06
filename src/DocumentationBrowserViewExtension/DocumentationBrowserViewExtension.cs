using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using DesignScript.Builtin;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// The DocumentationBrowser view extension displays web or local html files on the Dynamo right panel.
    /// It reacts to documentation display request events in Dynamo to know what and when to display documentation.
    /// </summary>
    public class DocumentationBrowserViewExtension : ViewExtensionBase, IViewExtension, ILogSource, ILayoutSpecSource
    {
        private ViewStartupParams viewStartupParamsReference;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem documentationBrowserMenuItem;
        private PackageManagerExtension pmExtension;
        private const string FALLBACK_DOC_DIRECTORY_NAME = "fallback_docs";
        //these fields should only be directly set by tests.
        internal DirectoryInfo fallbackDocPath;
        internal DirectoryInfo webBrowserUserDataFolder;

        internal Dictionary<string, string> BreadCrumbsDict { get; set; }

        internal DocumentationBrowserView BrowserView { get; private set; }
        internal DocumentationBrowserViewModel ViewModel { get; private set; }

        /// <summary>
        /// Extension Name
        /// </summary>
        public override string Name => Properties.Resources.ExtensionName;

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public override string UniqueId => "68B45FC0-0BD1-435C-BF28-B97CB03C71C8";

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

        private Func<LayoutSpecification> layouthandler;

        public event Action<string> RequestApplyLayoutSpec;

        public event Func<LayoutSpecification> RequestLayoutSpec
        {
            add { layouthandler += value; }
            remove { layouthandler -= value; }
        }

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            this.viewStartupParamsReference = viewStartupParams;

            pmExtension = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            PackageDocumentationManager.Instance.AddDynamoPaths(viewStartupParams.PathManager);

            var pathManager = viewStartupParams.PathManager;
            if (!string.IsNullOrEmpty(pathManager.DynamoCoreDirectory))
            {
                var docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, FALLBACK_DOC_DIRECTORY_NAME));
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }

            if (!string.IsNullOrEmpty(pathManager.HostApplicationDirectory))
            {
                //when running over any host app like Revit, FormIt, Civil3D... the path to the fallback_docs can change.
                //e.g. for Revit the variable HostApplicationDirectory = C:\Program Files\Autodesk\Revit 2023\AddIns\DynamoForRevit\Revit
                //Then we need to remove the last folder from the path so we can find the fallback_docs directory.
                var hostAppDirectory = Directory.GetParent(pathManager.HostApplicationDirectory).FullName;
                var docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, FALLBACK_DOC_DIRECTORY_NAME));
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }

            //When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
            var userDataDir = new DirectoryInfo(pathManager.UserDataDirectory);
            webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            if (this.BrowserView == null) return;

            if(fallbackDocPath != null)
            {
                this.BrowserView.FallbackDirectoryName = fallbackDocPath.FullName;
            }

            if(webBrowserUserDataFolder != null)
            {
                this.BrowserView.WebBrowserUserDataFolder = webBrowserUserDataFolder.FullName;
            }
            
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (viewLoadedParams == null) throw new ArgumentNullException(nameof(viewLoadedParams));
            this.viewLoadedParamsReference = viewLoadedParams; 

            this.ViewModel.MessageLogged += OnViewModelMessageLogged;
            PackageDocumentationManager.Instance.MessageLogged += OnMessageLogged;


            // Add a button to Dynamo View menu to manually show the window
            this.documentationBrowserMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
            this.documentationBrowserMenuItem.Checked += MenuItemCheckHandler;
            this.documentationBrowserMenuItem.Unchecked += MenuItemUnCheckedHandler;
            this.viewLoadedParamsReference.AddExtensionMenuItem(this.documentationBrowserMenuItem);

            // subscribe to the documentation open request event from Dynamo
            this.viewLoadedParamsReference.RequestOpenDocumentationLink += HandleRequestOpenDocumentationLink;

            // subscribe to property changes of DynamoViewModel so we can show/hide the browser on StartPage display
            (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged += HandleStartPageVisibilityChange;

            // pmExtension could be null, if this is the case we bail before interacting with it.
            if (pmExtension is null)
                return;

            // subscribe to package loaded so we can add the package documentation 
            // to the Package documentation manager when a package is loaded
            pmExtension.PackageLoader.PackgeLoaded += OnPackageLoaded;

            // add packages already loaded to the PackageDocumentationManager
            foreach (var pkg in pmExtension.PackageLoader.LocalPackages)
            {
                OnPackageLoaded(pkg);
            }
            
        }

        private void RequestLoadLayoutSpecs()
        {
            if (BreadCrumbsDict != null) return;

            var output = layouthandler?.Invoke();
            if (output == null) return;

            PopulateBreadCrumbsDictionary(output);
        }

        private void PopulateBreadCrumbsDictionary(LayoutSpecification layoutSpec)
        {
            var section = layoutSpec.sections.First();
            var breadCrumb = string.Empty;

            BreadCrumbsDict = new Dictionary<string, string>();

            if (section.childElements.Count == 0) return;

            foreach (var child in section.childElements)
            {
                breadCrumb = child.text + " / ";

                RecursiveIncludeSearch(child, breadCrumb);
            }

            this.ViewModel.BreadCrumbsDictionary = BreadCrumbsDict;
        }
        
        private void RecursiveIncludeSearch(LayoutElement child, string breadCrumb)
        {
            string crumb = breadCrumb;

            if (child.childElements.Any())
            {
                foreach (var grandchild in child.childElements)
                {
                    crumb = breadCrumb + grandchild.text + " / ";

                    RecursiveIncludeSearch(grandchild, crumb);
                }
            }

            foreach (var info in child.include)
            {
                var typeArray = info.path.Split('.');
                var type = typeArray[typeArray.Length - 2] + "." + typeArray[typeArray.Length - 1];

                BreadCrumbsDict[type] = crumb.Remove(crumb.Length-3);
            }
        }


        private void OnPackageLoaded(Package pkg)
        {
            // Add documentation files from the package to the DocManager
            PackageDocumentationManager.Instance.AddPackageDocumentation(pkg.NodeDocumentaionDirectory, pkg.Name);
        }

        private void MenuItemUnCheckedHandler(object sender, RoutedEventArgs e)
        {
            viewLoadedParamsReference.CloseExtensioninInSideBar(this);
        }

        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {

            AddToSidebar(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            if (this.viewLoadedParamsReference != null)
            {
                this.viewLoadedParamsReference.RequestOpenDocumentationLink -= HandleRequestOpenDocumentationLink;
            }

            if (this.ViewModel != null)
            {
                this.ViewModel.MessageLogged -= OnViewModelMessageLogged;
            }

            if (this.documentationBrowserMenuItem != null)
            {
                this.documentationBrowserMenuItem.Checked -= MenuItemCheckHandler;
                this.documentationBrowserMenuItem.Unchecked -= MenuItemUnCheckedHandler;
            }

            this.BrowserView?.Dispose();
            this.ViewModel?.Dispose();

            if (this.viewLoadedParamsReference != null)
            {
                (this.viewLoadedParamsReference.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged -=
                    HandleStartPageVisibilityChange;
            }

            if (this.pmExtension != null)
            {
                this.pmExtension.PackageLoader.PackgeLoaded -= OnPackageLoaded;
            }
            PackageDocumentationManager.Instance.Dispose();
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public override void Dispose()
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

            // make sure the breadcrumbs dictionary has been loaded
            RequestLoadLayoutSpecs();

            // make sure the view is added to the Sidebar
            // this also forces the Sidebar to open
            AddToSidebar(false);

            // forward the event to the ViewModel to handle
            this.ViewModel?.HandleOpenDocumentationLinkEvent(args);

            // Check the menu item
            this.documentationBrowserMenuItem.IsChecked = true;
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

        public override void Closed()
        {
            if (this.documentationBrowserMenuItem != null)
            {
                this.documentationBrowserMenuItem.IsChecked = false;
            }
        }

    }
}
