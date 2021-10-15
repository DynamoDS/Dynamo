using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Web;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using DynamoUtilities;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class WorkspaceDependencyView : UserControl, IDisposable
    {

        private WorkspaceModel currentWorkspace;

        /// <summary>
        /// The hyper link where Dynamo user will be forwarded to for submitting comments.
        /// </summary>
        private readonly string FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";
        private readonly string customNodeExtension = ".dyf";

        private ViewLoadedParams loadedParams;
        private WorkspaceDependencyViewExtension dependencyViewExtension;

        private IPackageInstaller packageInstaller;

        /// <summary>
        /// Internal cache of the data displayed in data grid, useful in unit testing.
        /// You are not expected to modify this but rather inspection.
        /// </summary>
        internal IEnumerable<PackageDependencyRow> dataRows;
        internal IEnumerable<DependencyRow> localDefinitionDataRows;
        internal IEnumerable<DependencyRow> externalFilesDataRows;

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
                    if (dependencyViewExtension.workspaceReferencesMenuItem != null && !dependencyViewExtension.workspaceReferencesMenuItem.IsChecked)
                    {
                        dependencyViewExtension.workspaceReferencesMenuItem.IsChecked = true;
                    }
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
                System.Diagnostics.Process.Start(FeedbackLink);
            }
            catch (Exception ex)
            {
                String message = Dynamo.WorkspaceDependency.Properties.Resources.ProvideFeedbackError + "\n\n" + ex.Message;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                DependencyRegen(obj as WorkspaceModel);
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
            PackageDependencyTable.ItemsSource = null;
            if (obj is WorkspaceModel)
            {
                DependencyRegen(obj as WorkspaceModel);
            }
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(currentWorkspace.NodeLibraryDependencies) || args.PropertyName == nameof(currentWorkspace.NodeLocalDefinitions) || args.PropertyName == nameof(currentWorkspace.ExternalFiles))
                DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Regenerate dependency table
        /// </summary>
        /// <param name="ws">workspace model</param>
        internal void DependencyRegen(WorkspaceModel ws)
        {
            RestartBanner.Visibility = Visibility.Hidden;
            var packageDependencies = ws.NodeLibraryDependencies.Where(d => d is PackageDependencyInfo).ToList();
            var localDefinitions = ws.NodeLocalDefinitions.Where(d => d is DependencyInfo).ToList();
            var externalFiles = ws.ExternalFiles.Where(d => d is DependencyInfo).ToList();

            foreach (DependencyInfo info in localDefinitions)
            {
                try
                {
                    if (info.ReferenceType == ReferenceType.DYFFile)
                    {
                        // Try to get the Custom node information if possible.
                        string customNodeName = info.Name.Replace(customNodeExtension, "");
                        dependencyViewExtension.DependencyView.CustomNodeManager.TryGetNodeInfo(customNodeName, out CustomNodeInfo customNodeInfo);

                        if (customNodeInfo != null)
                        {
                            info.Path = customNodeInfo.Path;
                        }
                    }

                    info.Size = PathHelper.GetFileSize(info.Path);
                }
                catch (Exception ex)
                {
                    dependencyViewExtension.OnMessageLogged(LogMessage.Info(string.Format(Properties.Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
                }

                HasDependencyIssue = info.Path == null;
            }

            foreach (DependencyInfo info in externalFiles)
            {
                HasDependencyIssue = info.Path == null;
            }

            var pythonPackageDependencies = ws.OnRequestPackageDependencies();
            if (pythonPackageDependencies != null)
                packageDependencies.AddRange(pythonPackageDependencies);

            if (packageDependencies.Any(d => d.State != PackageDependencyState.Loaded))
            {
                HasDependencyIssue = true;
            }

            if (packageDependencies.Any())
            {
                Boolean hasPackageMarkedForUninstall = false;
                // If package is set to uninstall state, update the package info
                foreach (var package in dependencyViewExtension.pmExtension.PackageLoader.LocalPackages.Where(x => 
                x.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForDeletion || x.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForUnload))
                {
                    (packageDependencies.FirstOrDefault(x => x.Name == package.Name) as PackageDependencyInfo).State =
                        PackageDependencyState.RequiresRestart;
                    hasPackageMarkedForUninstall = true;
                }

                RestartBanner.Visibility = hasPackageMarkedForUninstall ? Visibility.Visible: Visibility.Hidden;
            }

            var pmExtension = dependencyViewExtension.pmExtension;
            if (pmExtension != null)
            {
                foreach (PackageDependencyInfo packageDependencyInfo in packageDependencies)
                {
                    var targetInfo = pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == packageDependencyInfo.Name).FirstOrDefault();
                    if (targetInfo != null)
                    {
                        packageDependencyInfo.Path = targetInfo.RootDirectory;
                    }
                }
            }

            dataRows = packageDependencies.Select(d => new PackageDependencyRow(d as PackageDependencyInfo));
            localDefinitionDataRows = localDefinitions.Select(d => new DependencyRow(d as DependencyInfo));
            externalFilesDataRows = externalFiles.Select(d => new DependencyRow(d as DependencyInfo));

            Packages.IsExpanded = dataRows.Count() > 0;
            LocalDefinitions.IsExpanded = localDefinitionDataRows.Count() > 0;
            ExternalFiles.IsExpanded = externalFilesDataRows.Count() > 0;


            PackageDependencyTable.ItemsSource = dataRows;
            LocalDefinitionsTable.ItemsSource = localDefinitionDataRows;
            ExternalFilesTable.ItemsSource = externalFilesDataRows;
        }

        /// <summary>
        /// Calls the DependencyRegen function when the DummyNodesReloaded event is triggered from the dynamo model.
        /// </summary>
        internal void TriggerDependencyRegen()
        {
            DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public WorkspaceDependencyView(WorkspaceDependencyViewExtension viewExtension, ViewLoadedParams p)
        {
            InitializeComponent();
            this.DataContext = this;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            WorkspaceModel.DummyNodesReloaded += TriggerDependencyRegen;
            currentWorkspace.Saved += TriggerDependencyRegen;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            loadedParams = p;
            packageInstaller = p.PackageInstaller;
            dependencyViewExtension = viewExtension;
            DependencyRegen(currentWorkspace);
            HomeWorkspaceModel.WorkspaceClosed += this.CloseExtensionTab;
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
                var info = ((PackageDependencyRow)((Button)sender).DataContext).DependencyInfo;
                DownloadSpecifiedPackageAndRefresh(info);
            }
            catch (Exception ex)
            {
                dependencyViewExtension.OnMessageLogged(LogMessage.Info(String.Format(Properties.Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
            }
        }

        /// <summary>
        /// Downloaded the specified package according to serialized dyn
        /// and refresh the view of dependency viewer
        /// </summary>
        /// <param name="info">Target PackageDependencyInfo to download</param>
        internal void DownloadSpecifiedPackageAndRefresh(PackageDependencyInfo info)
        {
            packageInstaller.DownloadAndInstallPackage(info);
            DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Handler of button which user click when choosing to keep the
        /// installed version of package instead of the specified one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeepLocalPackage(object sender, RoutedEventArgs e)
        {
            try
            {
                var info = ((PackageDependencyRow)((Button)sender).DataContext).DependencyInfo;
                UpdateWorkspaceToUseInstalledPackage(info);
            }
            catch (Exception ex)
            {
                dependencyViewExtension.OnMessageLogged(LogMessage.Info(String.Format(Properties.Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
            }
        }

        /// <summary>
        /// Update current workspace in memory to keep the installed package
        /// instead of keep referencing the dependency info saved in DYN
        /// </summary>
        /// <param name="info">Target PackageDependencyInfo to update version</param>
        internal void UpdateWorkspaceToUseInstalledPackage(PackageDependencyInfo info)
        {
            var pmExtension = dependencyViewExtension.pmExtension;
            if (pmExtension != null)
            {
                var targetInfo = pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == info.Name).FirstOrDefault();
                if (targetInfo != null)
                {
                    info.Version = new Version(targetInfo.VersionName);
                    info.State = PackageDependencyState.Loaded;
                    info.Path = targetInfo.RootDirectory;
                    // Mark the current workspace dirty for save
                    currentWorkspace.HasUnsavedChanges = true;
                    DependencyRegen(currentWorkspace);
                }
            }
        }

        /// <summary>
        /// Dispose function for WorkspaceDependencyView
        /// </summary>
        public void Dispose()
        {
            loadedParams.CurrentWorkspaceChanged -= OnWorkspaceChanged;
            loadedParams.CurrentWorkspaceCleared -= OnWorkspaceCleared;
            currentWorkspace.PropertyChanged -= OnWorkspacePropertyChanged;
            WorkspaceModel.DummyNodesReloaded -= TriggerDependencyRegen;
            currentWorkspace.Saved -= TriggerDependencyRegen;
            HomeWorkspaceModel.WorkspaceClosed -= this.CloseExtensionTab;
            PackageDependencyTable.ItemsSource = null;
            LocalDefinitionsTable.ItemsSource = null;
            ExternalFilesTable.ItemsSource = null;
            dataRows = null;
            localDefinitionDataRows = null;
            externalFilesDataRows = null;
        }

        private void Refresh_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyRegen(currentWorkspace);
        }
    }

    /// <summary>
    /// Represents information about a package dependency as a row in the dependency table
    /// </summary>
    public class PackageDependencyRow
    {
        internal PackageDependencyInfo DependencyInfo { get; private set; }

        internal PackageDependencyRow(PackageDependencyInfo nodeLibraryDependencyInfo)
        {
            DependencyInfo = nodeLibraryDependencyInfo;
        }

        /// <summary>
        /// Name of this package dependency
        /// </summary>
        public string Name => DependencyInfo.Name;

        /// <summary>
        /// {ackage dependency path
        /// </summary>
        public string Path => DependencyInfo.Path;

        /// <summary>
        /// Version of this package dependency
        /// </summary>
        public Version Version => DependencyInfo.Version;

        /// <summary>
        /// The message to be displayed in the expanded details section for this package dependency.
        /// This message describes the state of the package and possible user actions of the dependency.
        /// </summary>
        public string DetailsMessage
        {
            get
            {
                string message;

                switch (DependencyInfo.State)
                {
                    case PackageDependencyState.Loaded:
                        message = string.Format(Properties.Resources.DetailsMessageLoaded,
                            DependencyInfo.Name, DependencyInfo.Version.ToString());
                        break;

                    case PackageDependencyState.Missing:
                        message = string.Format(Properties.Resources.DetailsMessageMissing,
                            DependencyInfo.Name, DependencyInfo.Version.ToString());
                        break;

                    case PackageDependencyState.IncorrectVersion:
                        message = string.Format(Properties.Resources.DetailsMessageIncorrectVersion,
                            DependencyInfo.Name, DependencyInfo.Version.ToString());
                        break;

                    case PackageDependencyState.RequiresRestart:
                        message = string.Format(Properties.Resources.DetailsMessageRequireRestart);
                        break;

                    default:
                        message = string.Format(Properties.Resources.DetailsMessageWarning,
                            DependencyInfo.Name, DependencyInfo.Version.ToString());
                        break;
                }

                return message;
            }
        }

        /// <summary>
        /// The icon representing the state of this package dependency
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                Bitmap bitmap;

                switch (DependencyInfo.State)
                {
                    case PackageDependencyState.Loaded:
                        bitmap = Properties.Resources.NodeLibraryDependency_Loaded;
                        break;

                    case PackageDependencyState.Missing:
                        bitmap = Properties.Resources.NodeLibraryDependency_Missing;
                        break;

                    case PackageDependencyState.RequiresRestart:
                        bitmap = Properties.Resources.NodeLibraryDependency_Warning;
                        break;

                    default:
                        bitmap = Properties.Resources.NodeLibraryDependency_Warning;
                        break;
                }

                return ResourceUtilities.ConvertToImageSource(bitmap);
            }
        }

        /// <summary>
        /// Indicates whether to show/enable the package download and install button
        /// </summary>
        public bool ShowDownloadButton => this.DependencyInfo.State == PackageDependencyState.Missing || this.DependencyInfo.State == PackageDependencyState.IncorrectVersion;

        /// <summary>
        /// Indicates whether to show/enable the package keep local button
        /// </summary>
        public bool ShowKeepLocalButton => this.DependencyInfo.State == PackageDependencyState.IncorrectVersion;
    }

    /// <summary>
    /// Represents information about a dependency as a row in the dependency table
    /// </summary>
    public class DependencyRow
    {
        internal DependencyInfo DependencyInfo { get; private set; }

        internal DependencyRow(DependencyInfo localDefinitionInfo)
        {
            DependencyInfo = localDefinitionInfo;
        }

        /// <summary>
        /// Name of this dependency
        /// </summary>
        public string Name => DependencyInfo.Name;

        /// <summary>
        /// local dependency path
        /// </summary>
        public string Path => DependencyInfo.Path;

        /// <summary>
        /// local dependency size
        /// </summary>
        public string Size => DependencyInfo.Size;

        /// <summary>
        /// The icon representing the reference object.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                Bitmap bitmap = null;

                switch (DependencyInfo.ReferenceType)
                {
                    case ReferenceType.DYFFile:
                        bitmap = Properties.Resources.CustomNodeReferenceIcon;
                        break;

                    case ReferenceType.ZeroTouch:
                        bitmap = Properties.Resources.ZeroTouchNodeReferenceIcon;
                        break;

                    case ReferenceType.External when MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("image"):
                        bitmap = Properties.Resources.ImageIcon;
                        break;

                    case ReferenceType.External when MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("excel") || MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("spreadsheet"):
                        bitmap = Properties.Resources.ExcelIcon;
                        break;

                    case ReferenceType.External when MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("json"):
                        bitmap = Properties.Resources.JsonIcon;
                        break;

                    case ReferenceType.External when MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("pdf"):
                        bitmap = Properties.Resources.PDFIcon;
                        break;

                    case ReferenceType.External when MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("csv"):
                        bitmap = Properties.Resources.CSVIcon;
                        break;

                    case ReferenceType.External when MimeMapping.GetMimeMapping(DependencyInfo.Name).Contains("dwg"):
                        bitmap = Properties.Resources.DWGIcon;
                        break;

                    case ReferenceType.External:
                        bitmap = Properties.Resources.ExternalFileIcon;
                        break;
                }

                return ResourceUtilities.ConvertToImageSource(bitmap);
            }
        }
    }
}
