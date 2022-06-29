using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.GraphNodeManager.ViewModels;

namespace Dynamo.GraphNodeManager
{
    /// <summary>
    /// Interaction logic for GraphNodeManagerView.xaml
    /// </summary>
    public partial class GraphNodeManagerView : UserControl
    {
        /// <summary>
        /// A persistent handle of the currently selected row
        /// </summary>
        private DataGridRow selectedRow;

        /// <summary>
        /// Allows to handle the selection of the DataGrid element
        /// </summary>
        private bool allowSelection = false;

        /// <summary>
        /// Helps to stop bubbling of the event when mouse click on a button inside a datagrid row
        /// </summary>
        private bool mouseHandled = false;

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
            if (!allowSelection) return;
            allowSelection = false;

            var vm = this.DataContext as GraphNodeManagerViewModel;
            if ((sender as DataGrid) == null) return;
            vm.NodeSelect((sender as DataGrid).SelectedItem);
        }

        /// <summary>
        /// Handles export image click function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Handles DataGrid row click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_PreviewClickHandler(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandled)
            {
                mouseHandled = false;
                return;
            }

            DataGridRow row = sender as DataGridRow;
            if (row == null) return;
            if (selectedRow != null && row != selectedRow)
            {
                ResetRows();
            }

            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            allowSelection = true;

            if (row.DetailsVisibility == Visibility.Collapsed) selectedRow = null;
            else selectedRow = row;
        }

        /// <summary>
        /// Resets the selected row details visibility
        /// </summary>
        private void ResetRows()
        {
            if (selectedRow != null) selectedRow.DetailsVisibility = Visibility.Collapsed;
            selectedRow = null;
        }

        /// <summary>
        /// Handles Clipboard Button Click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClipboardButtonClick(object sender, RoutedEventArgs e)
        {
            mouseHandled = true;

            var node = (GridNodeViewModel)((Button)sender).Tag;
            var info = ((Button)sender).DataContext as NodeInfo;
            var vm = this.DataContext as GraphNodeManagerViewModel;

            var dynamoVersion = vm.DynamoVersion;
            var hostProgram = vm.HostName; // How do we get the Host Program? Wouldn't that be the same as Dynamo?

            // Package - package manager

            if (node == null || info == null) return;

            var message = $"Node Name: {node.Name}\nPackage: {node.Package}\nDynamo Version: {dynamoVersion}\nHost: {hostProgram}\nMessages: {info.Message}\nState: {info.State}";

            Clipboard.SetText(message);
        }
    }
}
