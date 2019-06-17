using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dynamo.PackageDependency
{
    /// <summary>
    /// Interaction logic for PackageDependencyView.xaml
    /// </summary>
    public partial class PackageDependencyView : UserControl
    {
        private DependencyTable table = new DependencyTable();

        private WorkspaceModel currentWorkspace;

        /// <summary>
        /// Event handler for workspaceAdded event
        /// </summary>
        /// <param name="obj"></param>
        internal void OnWorkspaceChanged(IWorkspaceModel obj)
        {
            if (obj is WorkspaceModel)
            {
                DependencyRegen(obj as WorkspaceModel);
                currentWorkspace = obj as WorkspaceModel;
                currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            }
        }

        /// <summary>
        /// Event handler for workspaceRemoved event
        /// </summary>
        /// <param name="obj"></param>
        internal void OnWorkspaceCleared(IWorkspaceModel obj)
        {
            if (obj is WorkspaceModel)
            {
                currentWorkspace.PropertyChanged -= OnWorkspacePropertyChanged;
                // Clear the dependency table.
                table.Columns.Clear();
            }
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(currentWorkspace.PackageDependencies))
                DependencyRegen(currentWorkspace);
        }

        /// <summary>
        /// Regenerate dependency table
        /// </summary>
        /// <param name="ws"></param>
        private void DependencyRegen(WorkspaceModel ws)
        {
            // Clear the dependency table.
            table.Columns.Clear();
            foreach (var package in ws.PackageDependencies)
            {
                bool matchFound = false;
                // Check if the target package is installed and loaded.
                foreach (var loadedPackage in ws.LoadedPackageDependencies)
                {
                    if (package.Equals(loadedPackage))
                    {
                        table.Columns.Add(new Column()
                        {
                            ColumnsData = new ObservableCollection<ColumnData>()
                            {
                                new ColumnData(package.Name),
                                new ColumnData(package.Version.ToString(), 100)
                            }
                        });
                        matchFound = true;
                        continue;
                    }
                }
                // TODO: Not ideal! O(N * M) complexicty, would like LoadedPackageDependencies to be a dictionary or something with constant package name search time
                if (!matchFound)
                {
                    table.Columns.Add(new Column()
                    {
                        ColumnsData = new ObservableCollection<ColumnData>()
                        {
                            new ColumnData(package.Name, ColumnData.WarningColor),
                            new ColumnData(package.Version.ToString(), 100)
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoModel"></param>
        public PackageDependencyView(ViewLoadedParams p)
        {
            InitializeComponent();
            DataContext = table;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
        }
    }

    /// <summary>
    /// 
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
        public static Brush WarningColor = Brushes.Red;

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
