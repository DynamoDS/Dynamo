using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using Dynamo.Wpf.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class WorkspaceDependencyView : UserControl
    {

        private WorkspaceModel currentWorkspace;

        private String FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";

        private ViewLoadedParams loadedParams;
        private WorkspaceDependencyViewExtension dependencyViewExtension;

        private IPackageInstaller packageInstaller;

        private Boolean hasMissingPackage = false;

        /// <summary>
        /// Property to check if the current workspace has any missing package dependencies. 
        /// </summary>
        private Boolean HasMissingPackage
        {
            get { return hasMissingPackage; }
            set
            {
                hasMissingPackage = value;
                if (hasMissingPackage)
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
                String message = Dynamo.Wpf.Properties.Resources.ProvideFeedbackError + "\n\n" + ex.Message;
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

            if (packageDependencies.Any(d => d.State == PackageDependencyState.Missing))
            {
                HasMissingPackage = true;
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
    }

    public class PackageDependencyRow
    {
        internal PackageDependencyInfo DependencyInfo { get; private set; }

        internal PackageDependencyRow(PackageDependencyInfo nodeLibraryDependencyInfo)
        {
            DependencyInfo = nodeLibraryDependencyInfo;
        }

        public string Name => DependencyInfo.Name;

        public Version Version => DependencyInfo.Version;

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

                    default:
                        message = string.Format(Properties.Resources.DetailsMessageWarning, 
                            DependencyInfo.Name, DependencyInfo.Version.ToString());
                        break;
                }

                return message;
            }
        }

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

        public bool ShowDownloadButton => this.DependencyInfo.State == PackageDependencyState.Missing;
    }
}
