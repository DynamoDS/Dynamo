﻿using System;
using System.ComponentModel;
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

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class WorkspaceDependencyView : UserControl
    {

        private WorkspaceModel currentWorkspace;

        private string FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";

        private ViewLoadedParams loadedParams;
        private WorkspaceDependencyViewExtension dependencyViewExtension;

        private IPackageInstaller packageInstaller;

        private Boolean hasDependencyIssue = false;

        /// <summary>
        /// Property to check if the current workspace has any package dependencies
        /// issue worth workspace author's attention
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
                }
            }
        }

        /// <summary>
        /// Re-directs to a web link to get the feedback from the user. 
        /// </summary>
        private void ProvideFeedback(object sender, EventArgs e)
        {
            try {
                System.Diagnostics.Process.Start(FeedbackLink);
            }
            catch (Exception ex) {
                String message = Dynamo.WorkspaceDependency.Properties.Resources.ProvideFeedbackError + "\n\n" + ex.Message;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
            if (args.PropertyName == nameof(currentWorkspace.NodeLibraryDependencies))
                DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Regenerate dependency table
        /// </summary>
        /// <param name="ws">workspace model</param>
        internal void DependencyRegen(WorkspaceModel ws)
        {
            var packageDependencies = ws.NodeLibraryDependencies.Where(d => d is PackageDependencyInfo);

            if (packageDependencies.Any(d => d.State != PackageDependencyState.Loaded))
            {
                HasDependencyIssue = true;
            }

            PackageDependencyTable.ItemsSource = packageDependencies.Select(d => new PackageDependencyRow(d as PackageDependencyInfo));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public WorkspaceDependencyView(WorkspaceDependencyViewExtension viewExtension,ViewLoadedParams p)
        {
            InitializeComponent();
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            loadedParams = p;
            packageInstaller = p.PackageInstaller;
            dependencyViewExtension = viewExtension;
            DependencyRegen(currentWorkspace);
        }
        
        /// <summary>
        /// Send a request to the package manager client to download this package and its dependencies
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadPackage(object sender, RoutedEventArgs e)
        {
            var info = ((PackageDependencyRow)((Button)sender).DataContext).DependencyInfo;
            var package = new PackageInfo(info.Name, info.Version);

            packageInstaller.DownloadAndInstallPackage(package);
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
            catch(Exception ex)
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
                    // Mark the current workspace dirty for save
                    currentWorkspace.HasUnsavedChanges = true;
                    DependencyRegen(currentWorkspace);
                }
            }
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
}
