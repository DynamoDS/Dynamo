using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Wpf.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dynamo.PackageDependency
{
    /// <summary>
    /// Interaction logic for PackageDependencyView.xaml
    /// </summary>
    public partial class PackageDependencyView : UserControl
    {
        protected DependencyTable table = new DependencyTable();

        /// <summary>
        /// Event handler for workspaceAdded event
        /// </summary>
        /// <param name="obj"></param>
        internal void OnWorkspaceChanged(IWorkspaceModel obj)
        {
            // Clear the dependency table.
            table.Columns.Clear();
            if (obj is WorkspaceModel)
            {
                foreach (var package in (obj as WorkspaceModel).PackageDependencies)
                {
                    table.Columns.Add(new Column()
                    {
                        ColumnsData = new ObservableCollection<ColumnData>()
                    {
                        new ColumnData(package.Name),
                        new ColumnData(package.Version.ToString(), 100)
                    }
                    });
                }
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
                // Clear the dependency table.
                table.Columns.Clear();
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
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
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
        static int DefaultWidth = 150;
        static Brush DefaultColor = Brushes.Red;

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
