using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.GraphNodeManager
{
    /// <summary>
    /// Interaction logic for GraphNodeManagerView.xaml
    /// </summary>
    public partial class GraphNodeManagerView : UserControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewModel"></param>
        public GraphNodeManagerView(GraphNodeManagerViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        /// <summary>
        /// Handles selection changed of DataGrid Item
        /// Calls a method in the ViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as GraphNodeManagerViewModel;
            if ((sender as DataGrid) == null) return;
            vm.NodeSelect((sender as DataGrid).SelectedItem);
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as GraphNodeManagerViewModel;
            if (vm == null) return;

            MenuItem mi = sender as MenuItem;
            if (mi.Name.Equals("ExportToCSV")) vm.ExportGraph("CSV");
            if (mi.Name.Equals("ExportToJSON")) vm.ExportGraph("JSON");
        }

        private void ExportImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Image image = sender as Image;
                ContextMenu contextMenu = image.ContextMenu;
                contextMenu.PlacementTarget = image;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
    }
}
