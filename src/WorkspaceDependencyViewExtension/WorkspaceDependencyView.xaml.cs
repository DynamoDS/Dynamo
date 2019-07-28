using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class WorkspaceDependencyView : UserControl
    {

        private WorkspaceModel currentWorkspace;

        private PackageManagerClientViewModel packageManagerClientViewModel;

        private String FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";

        private ViewLoadedParams loadedParams;
        private WorkspaceDependencyViewExtension dependencyViewExtension;

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
            if (ws.NodeLibraryDependencies.Any(d => d.State == PackageDependencyState.Missing))
            {
                HasMissingPackage = true;
            }

            PackageDependencyTable.ItemsSource = ws.NodeLibraryDependencies.Select(d => new NodeLibraryDependencyRow(d));
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
            dependencyViewExtension = viewExtension;
            DependencyRegen(currentWorkspace);
            packageManagerClientViewModel = loadedParams.PackageManagerClientViewModel;
        }
        
        /// <summary>
        /// Send a request to the package manager client to download this package and its dependencies
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadPackage(object sender, RoutedEventArgs e)
        {
            var info = ((NodeLibraryDependencyRow)((Button)sender).DataContext).DependencyInfo;
            var package = new PackageInfo(info.Name, info.Version);

            packageManagerClientViewModel.InitiatePackageDownloadAndInstall(package);
            DependencyRegen(currentWorkspace);
        }
    }

    public class NodeLibraryDependencyRow
    {
        public INodeLibraryDependencyInfo DependencyInfo { get; private set; }

        public NodeLibraryDependencyRow(INodeLibraryDependencyInfo nodeLibraryDependencyInfo)
        {
            DependencyInfo = nodeLibraryDependencyInfo;
        }

        public string Name => DependencyInfo.Name;

        public Version Version => DependencyInfo.Version;

        public ImageSource Icon
        {
            get
            {
                string iconPath;

                switch (DependencyInfo.State)
                {
                    case PackageDependencyState.Loaded:
                        iconPath = "NodeLibraryDependency_Loaded.png";
                        break;

                    case PackageDependencyState.Missing:
                        iconPath = "NodeLibraryDependency_Missing.png";
                        break;

                    default:
                        iconPath = "NodeLibraryDependency_Warning.png";
                        break;
                }

                return LoadBitmapImage(iconPath); 
            }
        }

        public bool ShowDownloadButton => this.DependencyInfo.State == PackageDependencyState.Missing;

        private BitmapImage LoadBitmapImage(string iconPath)
        {
            var format = @"pack://application:,,,/WorkspaceDependencyViewExtension;component/Images/{0}";
            iconPath = string.Format(format, iconPath);
            return new BitmapImage(new Uri(iconPath, UriKind.Absolute));
        }

        public void InitiateDownloadAndInstall()
        {

        }
    }
}
