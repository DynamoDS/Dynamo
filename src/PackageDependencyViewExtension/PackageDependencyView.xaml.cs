using Dynamo.Graph.Workspaces;
using Dynamo.Models;
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

        internal IEnumerable<Graph.Workspaces.PackageDependencyInfo> packages;
        internal DependencyTable table = new DependencyTable();

        private DynamoModel dynamoModel;

        /// <summary>
        /// Event handler for workspaceAdded event
        /// </summary>
        /// <param name="obj"></param>
        public void WorkspaceOpened(WorkspaceModel obj)
        {
            packages = obj.PackageDependencies;
            foreach(var package in packages)
            {
                table.Columns.Add(new Column()
                {
                    ColumnsData = new ObservableCollection<ColumnData>()
                    {
                        new ColumnData(package.Name),
                        new ColumnData(package.Version.ToString())
                    }
                });
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoModel"></param>
        public PackageDependencyView(DynamoModel dynamoModel)
        {
            InitializeComponent();
            DataContext = table;
            // Initialize but usually empty at this point
            packages = dynamoModel.CurrentWorkspace.PackageDependencies;
            dynamoModel.WorkspaceAdded += WorkspaceOpened;
        }
    }
    public class Column
    {
        public ObservableCollection<ColumnData> ColumnsData
        {
            get; set;
        }
    }

    public class ColumnData
    {
        public int DefaultWidth = 100;
        public Brush DefaultColor = Brushes.Red;

        public ColumnData(string data)
        {
            Data = data;
            Width = DefaultWidth;
            Color = DefaultColor;
        }

        public string Data
        {
            get; set;
        }

        public int Width
        {
            get; set;
        }

        public Brush Color
        {
            get; set;
        }
    }

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
                        new ColumnData("SamplePackage 1"),
                        new ColumnData("1.0")
                    }
            });

            Headers.Add(new ColumnData("Package"));
            Headers.Add(new ColumnData("Version"));
        }
    }
}
