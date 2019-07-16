using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class WorkspaceDependencyView : UserControl
    {
        private DependencyTable table = new DependencyTable();

        private WorkspaceModel currentWorkspace;

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
            if (obj is WorkspaceModel)
            {
                // Clear the dependency table.
                table.Columns.Clear();
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
            // Clear the dependency table.
            table.Columns.Clear();

            foreach (var package in ws.NodeLibraryDependencies)
            {
                if (package is PackageDependencyInfo)
                {
                    PackageDependencyInfo packageDependency = (PackageDependencyInfo) package;

                    // Different states for dependency packages.
                    switch (packageDependency.State)
                    {
                        case PackageDependencyState.Loaded:
                            table.Columns.Add(new Column()
                            {
                                ColumnsData = new ObservableCollection<ColumnData>()
                                {
                                    new ColumnData(packageDependency.Name),
                                    new ColumnData(packageDependency.Version.ToString(), 100)
                                }
                            });
                            break;

                        case PackageDependencyState.IncorrectVersion:
                            HasMissingPackage = true;
                            table.Columns.Add(new Column()
                            {
                                ColumnsData = new ObservableCollection<ColumnData>()
                                {
                                    new ColumnData(packageDependency.Name),
                                    new ColumnData(packageDependency.Version.ToString(), 100, ColumnData.WarningColor)
                                }
                            });
                            break;

                        case PackageDependencyState.Missing:
                            HasMissingPackage = true;
                            table.Columns.Add(new Column()
                            {
                                ColumnsData = new ObservableCollection<ColumnData>()
                                {
                                    new ColumnData(packageDependency.Name, ColumnData.MissingColor),
                                    new ColumnData(packageDependency.Version.ToString(), 100, ColumnData.MissingColor)
                                }
                            });
                            break;

                        case PackageDependencyState.Warning:
                            HasMissingPackage = true;
                            table.Columns.Add(new Column()
                            {
                                ColumnsData = new ObservableCollection<ColumnData>()
                                {
                                    new ColumnData(packageDependency.Name, ColumnData.WarningColor),
                                    new ColumnData(packageDependency.Version.ToString(), 100, ColumnData.WarningColor)
                                }
                            });
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public WorkspaceDependencyView(WorkspaceDependencyViewExtension viewExtension,ViewLoadedParams p)
        {
            InitializeComponent();
            DataContext = table;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            loadedParams = p;
            dependencyViewExtension = viewExtension;
        }
    }

    /// <summary>
    /// Every table line
    /// </summary>
    public class Column
    {
        public ObservableCollection<ColumnData> ColumnsData
        {
            get; set;
        }
    }

    /// <summary>
    /// Class defining data for each column
    /// </summary>
    public class ColumnData
    {
        static int DefaultWidth = 200;
        static SolidColorBrush DefaultColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#aaaaaa"));
        public static Brush MissingColor = Brushes.Red;
        public static Brush IncorrectVersionColor = Brushes.Orange;
        public static Brush WarningColor = Brushes.Yellow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        public ColumnData(string data)
        {
            Data = data;
            Width = DefaultWidth;
            Color = DefaultColor;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        public ColumnData(string data, int width)
        {
            Data = data;
            Width = width;
            Color = DefaultColor;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="color"></param>
        public ColumnData(string data, Brush color)
        {
            Data = data;
            Width = DefaultWidth;
            Color = color;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="brush"></param>
        public ColumnData(string data, int width, Brush brush)
        {
            Data = data;
            Width = width;
            Color = brush;
        }

        /// <summary>
        /// Data in each cell
        /// </summary>
        public string Data
        {
            get; set;
        }

        /// <summary>
        /// Width of each cell
        /// </summary>
        public int Width
        {
            get; set;
        }

        /// <summary>
        /// Foreground color of each cell
        /// </summary>
        public Brush Color
        {
            get; set;
        }
    }

    /// <summary>
    /// The data binding table holding all the dependency info
    /// </summary>
    public class DependencyTable
    {
        public ObservableCollection<Column> Columns
        { get; set; }

        public ObservableCollection<ColumnData> Headers
        { get; set; }

        public DependencyTable()
        {
            Columns = new ObservableCollection<Column>();
            Headers = new ObservableCollection<ColumnData>();

            Columns.Add(new Column()
            {
                ColumnsData = new ObservableCollection<ColumnData>()
                    {
                        new ColumnData("DummyPackage"),
                        new ColumnData("1.0.0")
                    }
            });

            Headers.Add(new ColumnData("Package Name"));
            Headers.Add(new ColumnData("Version"));
        }
    }
}
