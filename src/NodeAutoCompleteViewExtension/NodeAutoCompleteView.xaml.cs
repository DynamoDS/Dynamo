using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.NodeAutoComplete;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Utilities;


namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class NodeAutoCompleteView : UserControl, IDisposable
    {

        internal WorkspaceModel currentWorkspace;

        /// <summary>
        /// The hyper link where Dynamo user will be forwarded to for submitting comments.
        /// </summary>
        private readonly string FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";

        internal ViewLoadedParams loadedParams;
        private readonly NodeAutoCompleteViewExtension dependencyViewExtension;

        private readonly IPackageInstaller packageInstaller;

        private Boolean hasDependencyIssue = false;

        /// <summary>
        /// Property to raise to author's attention, if the Dynamo active workspace has any missing dependencies.
        /// This determines if the package dep viewer will be injected into right panel.
        /// </summary>
        private Boolean HasDependencyIssue
        {
            get { return hasDependencyIssue; }
            set
            {
                hasDependencyIssue = value;
                if (hasDependencyIssue)
                {
                    loadedParams.AddToExtensionsSideBar(dependencyViewExtension, this);
                    /*if (dependencyViewExtension.workspaceReferencesMenuItem != null && !dependencyViewExtension.workspaceReferencesMenuItem.IsChecked)
                    {
                        dependencyViewExtension.workspaceReferencesMenuItem.IsChecked = true;
                    }*/
                }
            }
        }

        /// <summary>
        /// Re-directs to a web link to get the feedback from the user. 
        /// </summary>
        private void ProvideFeedback(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo(FeedbackLink) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                //String message = Dynamo.WorkspaceDependency.Properties.Resources.ProvideFeedbackError + "\n\n" + ex.Message;
                //MessageBoxService.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal CustomNodeManager CustomNodeManager { get; set; }

        /// <summary>
        /// Event handler for workspaceAdded event
        /// </summary>
        /// <param name="obj">workspace model</param>
        internal void OnWorkspaceChanged(IWorkspaceModel obj)
        {
            if (obj is WorkspaceModel)
            {
                // Unsubscribe
                if (currentWorkspace != null)
                {
                    currentWorkspace.PropertyChanged -= OnWorkspacePropertyChanged;
                }
                //dependencyViewExtension.DependencyRegen(obj as WorkspaceModel, true);
                // Update current workspace
                currentWorkspace = obj as WorkspaceModel;
                currentWorkspace.Saved += TriggerDependencyRegen;
                currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            }
        }

        /// <summary>
        /// Event handler for workspaceRemoved event
        /// </summary>
        /// <param name="obj">workspace model</param>
        internal void OnWorkspaceCleared(IWorkspaceModel obj)
        {
            //PackageDependencyTable.ItemsSource = null;
            if (obj is WorkspaceModel)
            {
                //dependencyViewExtension.DependencyRegen(obj as WorkspaceModel);
            }
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            //if (args.PropertyName == nameof(currentWorkspace.NodeLibraryDependencies) || args.PropertyName == nameof(currentWorkspace.NodeLocalDefinitions) || args.PropertyName == nameof(currentWorkspace.ExternalFiles))
                //dependencyViewExtension.DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Calls DependencyRegen when workspace is saved
        /// </summary>
        internal void TriggerDependencyRegen()
        {
            //dependencyViewExtension.DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Calls DependencyRegen with forceCompute as true, as dummy nodes are reloaded.
        /// </summary>
        internal void ForceTriggerDependencyRegen()
        {
            //dependencyViewExtension.DependencyRegen(currentWorkspace, true);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public NodeAutoCompleteView(NodeAutoCompleteViewExtension viewExtension, ViewLoadedParams p)
        {
            //InitializeComponent();
            this.DataContext = this;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            WorkspaceModel.DummyNodesReloaded += ForceTriggerDependencyRegen;
            currentWorkspace.Saved += TriggerDependencyRegen;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            loadedParams = p;
            packageInstaller = p.PackageInstaller;
            dependencyViewExtension = viewExtension;
            //HomeWorkspaceModel.WorkspaceClosed += this.CloseExtensionTab;
        }

        /// <summary>
        /// This method will call the close API on the workspace references extension. 
        /// </summary>
        internal void CloseExtensionTab()
        {
            loadedParams.CloseExtensioninInSideBar(dependencyViewExtension);
        }

        /// <summary>
        /// Send a request to the package manager client to download this package and its dependencies
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadPackage(object sender, RoutedEventArgs e)
        {
            try
            {
                //var info = ((PackageDependencyRow)((Button)sender).DataContext).DependencyInfo;
                //DownloadSpecifiedPackageAndRefresh(info);
                Analytics.TrackEvent(Actions.DownloadNew, Categories.WorkspaceReferencesOperations);
            }
            catch (Exception ex)
            {
                //dependencyViewExtension.OnMessageLogged(LogMessage.Info(string.Format(Properties.Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
            }
        }

        /// <summary>
        /// Downloaded the specified package according to serialized dyn
        /// and refresh the view of dependency viewer
        /// </summary>
        /// <param name="info">Target PackageDependencyInfo to download</param>
        /*internal void DownloadSpecifiedPackageAndRefresh(PackageDependencyInfo info)
        {
            packageInstaller.DownloadAndInstallPackage(info);
        }*/

        /// <summary>
        /// Dispose function for WorkspaceDependencyView
        /// </summary>
        public void Dispose()
        {
            loadedParams.CurrentWorkspaceChanged -= OnWorkspaceChanged;
            loadedParams.CurrentWorkspaceCleared -= OnWorkspaceCleared;
            currentWorkspace.PropertyChanged -= OnWorkspacePropertyChanged;
            WorkspaceModel.DummyNodesReloaded -= ForceTriggerDependencyRegen;
            currentWorkspace.Saved -= TriggerDependencyRegen;
            /*HomeWorkspaceModel.WorkspaceClosed -= this.CloseExtensionTab;
            PackageDependencyTable.ItemsSource = null;
            LocalDefinitionsTable.ItemsSource = null;
            ExternalFilesTable.ItemsSource = null;*/
        }

        private void Refresh_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //dependencyViewExtension.DependencyRegen(currentWorkspace);
        }

        private void ForceRefresh_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //dependencyViewExtension.DependencyRegen(currentWorkspace, true);
        }
    }

}
