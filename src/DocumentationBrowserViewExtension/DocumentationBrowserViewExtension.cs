using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;
using DynamoProperties = Dynamo.Properties;
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
        private DynamoViewModel DynamoViewModel { get; set; }

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

        // Interface implementation allowing us to subscribe to the LayoutSpecification handler 
        public event Action<string> RequestApplyLayoutSpec;
        // Interface implementation allowing us to subscribe to the LayoutSpecification handler 
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
                var docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!docsDir.Exists)
                {
                    docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }

            if (!string.IsNullOrEmpty(pathManager.HostApplicationDirectory))
            {
                //when running over any host app like Revit, FormIt, Civil3D... the path to the fallback_docs can change.
                //e.g. for Revit the variable HostApplicationDirectory = C:\Program Files\Autodesk\Revit 2023\AddIns\DynamoForRevit\Revit
                //Then we need to remove the last folder from the path so we can find the fallback_docs directory.
                var hostAppDirectory = Directory.GetParent(pathManager.HostApplicationDirectory).FullName;
                var docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!docsDir.Exists)
                {
                    docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
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
            this.ViewModel.HandleInsertFile += OnInsertFile;
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

            this.DynamoViewModel = (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel);

            // set the viewmodel UIElement property to be used for Nodes Library manipulation
            this.ViewModel.DynamoView = viewLoadedParams.DynamoWindow;
        }

        public override void Shutdown()
        {
            Dispose();
        }

        private void OnInsertFile(object sender, InsertDocumentationLinkEventArgs e)
        {
            if (e.Data.Equals(Resources.FileNotFoundFailureMessage))
            {
                var message = String.Format(Resources.ToastFileNotFoundLocationNotificationText, e.Name);
                DynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(message, true);

                return;
            }

            if (DynamoViewModel.Model.CurrentWorkspace is HomeWorkspaceModel)
            {
                var homeWorkspace = DynamoViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
                if (homeWorkspace != null && homeWorkspace.RunSettings.RunType != RunType.Manual)
                {
                    DynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(Resources.ToastInsertGraphNotificationText, true);
                    homeWorkspace.RunSettings.RunType = RunType.Manual;
                }
            }
            
            var existingGroups = GetExistingGroups();

            // Insert the file and select all the elements that were inserted 
            this.DynamoViewModel.Model.InsertFileFromPath(e.Data);

            if (!DynamoSelection.Instance.Selection.Any()) return;

            GroupInsertedGraph(existingGroups, e.Name);
            DoEvents();

            // We have selected all the nodes and notes from the inserted graph
            // Now is the time to auto layout the inserted nodes
            this.DynamoViewModel.GraphAutoLayoutCommand.Execute(null);
            this.DynamoViewModel.FitViewCommand.Execute(false);
        }


        private void GroupInsertedGraph(List<AnnotationViewModel> existingGroups, string graphName)
        {
            var selection = GetCurrentSelection();
            var hostGroups = GetAllHostingGroups(existingGroups);

            foreach (var group in hostGroups)
            {
                group.DissolveNestedGroupsCommand.Execute(null);
            }

            foreach (var group in hostGroups)
            {
                selection.RemoveAll(x => group.AnnotationModel.ContainsModel(x as ModelBase));
            }
            
            DynamoSelection.Instance.Selection.AddRange(selection);
            DynamoSelection.Instance.Selection.AddRange(hostGroups.Select(x => x.AnnotationModel));

            // Add the inserted nodes into a group
            var annotation = this.DynamoViewModel.Model.CurrentWorkspace.AddAnnotation(Resources.InsertedGroupSubTitle, Guid.NewGuid());
            if (annotation != null)
            {
                annotation.AnnotationText = graphName;

                var annotationViewModel = DynamoViewModel.CurrentSpaceViewModel.Annotations
                        .First(x => x.AnnotationModel == annotation);

                var styleItem = annotationViewModel.GroupStyleList.First(x => x.Name.Equals(DynamoProperties.Resources.GroupStyleDefaultReview));
                var groupStyleItem = new GroupStyleItem {Name = styleItem.Name, HexColorString = styleItem.HexColorString};
                annotationViewModel.UpdateGroupStyle(groupStyleItem);

                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.AddRange(annotation.Nodes);
                DynamoSelection.Instance.Selection.Add(annotation);

                if (annotation.HasNestedGroups)
                {
                    DynamoSelection.Instance.Selection.AddRange(annotation.Nodes.OfType<AnnotationModel>().SelectMany(x => x.Nodes));
                }
            }
        }

        private List<AnnotationViewModel> GetAllHostingGroups(List<AnnotationViewModel> existingGroups)
        {
            List<AnnotationViewModel> hostGroups = new List<AnnotationViewModel>();

            foreach (var group in this.DynamoViewModel.CurrentSpaceViewModel.Annotations)
            {
                if (existingGroups.Contains(group)) continue;
                if (group.AnnotationModel.HasNestedGroups)
                {
                    hostGroups.Add(group);
                }
            }

            return hostGroups;
        }

        /// <summary>
        /// This method will return a reorganized version of the current selection
        /// discarding nodes if they are in groups, selecting the group instead
        /// </summary>
        /// <returns></returns>
        private List<ISelectable> GetCurrentSelection()
        {
            List<ISelectable> selection = new List<ISelectable>();

            foreach (var selected in DynamoSelection.Instance.Selection)
            {
                var nodeOrGroup = SelectNodeOrGroup(selected, selection);
                if (nodeOrGroup != null)
                {
                    selection.Add(nodeOrGroup);
                }
            }

            return selection;
        }

        private ISelectable SelectNodeOrGroup(ISelectable selected, List<ISelectable> selection)
        {
            foreach (var group in this.DynamoViewModel.CurrentSpaceViewModel.Annotations)
            {
                // Check if the current selected element is part of a group
                if (group.Nodes.Contains(selected))
                {
                    // If that's the case, and the group is not part of the selection set yet, add the group to the selection set
                    if (!selection.Contains(group.AnnotationModel))
                    {
                        return group.AnnotationModel;
                    }
                    // Else (if the element is part of a group, and the group has been added already to the selection set) skip this iteration
                    return null;
                }
            }
            // if the element was not part of a group, add it to the selection set
            return selected;
        }

        private List<AnnotationViewModel> GetExistingGroups()
        {
            List<AnnotationViewModel> result = new List<AnnotationViewModel>();

            foreach (var group in this.DynamoViewModel.CurrentSpaceViewModel.Annotations)
            {
                result.Add(group);
            }

            return result;
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
            BreadCrumbsDict = new Dictionary<string, string>();

            if (layoutSpec == null || !layoutSpec.sections.Any())
            {
                return;
            }
            var section = layoutSpec.sections.First();
            var breadCrumb = string.Empty;


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
                this.ViewModel.HandleInsertFile -= OnInsertFile;
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

        #region helper methods

        /// <summary>
        ///     Force the Dispatcher to empty it's queue
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        ///     Helper method for DispatcherUtil
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }

        #endregion

    }
}
