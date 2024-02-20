using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private GraphNodeManagerViewModel viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewModel"></param>
        public GraphNodeManagerView(GraphNodeManagerViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            this.DataContext = viewModel;

            viewModel.PropertyChanged += ViewModel_OnPropertyChanged;
            viewModel.RequestExportGraph += ViewModel_RequestExportGraph;
        }

        private void ViewModel_RequestExportGraph(object parameter)
        {
            if (parameter == null) return;
            var type = parameter.ToString();
            var promptName = System.IO.Path.GetFileNameWithoutExtension(viewModel.CurrentWorkspace.FileName);

            var filteredNodes = NodesInfoDataGrid.ItemsSource.Cast<GridNodeViewModel>().ToArray();

            switch (type)
            {
                case "CSV":
                    Utilities.Utilities.ExportToCSV(filteredNodes, promptName);
                    break;
                case "JSON":
                    Utilities.Utilities.ExportToJson(filteredNodes, promptName);
                    break;
            }
        }

        private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GraphNodeManagerViewModel.IsAnyFilterOn))
            {
                CollectionViewSource.GetDefaultView(NodesInfoDataGrid.ItemsSource).Refresh();
            }
        }

        /// <summary>
        /// Handles DataGrid row click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_PreviewClickHandler(object sender, MouseButtonEventArgs e)
        {
            FocusNode(sender);  // Always zoom around the node

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

        private void FocusNode(object sender)
        {
            var vm = this.DataContext as GraphNodeManagerViewModel;
            if ((sender as DataGridRow) == null) return;
            vm.NodeSelect((sender as DataGridRow).Item);
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

            var message = $"Node Name: {node.Name}\nOriginal Node Name: {node.OriginalName}\nPackage: {node.Package}\nDynamo Version: {dynamoVersion}\nHost: {hostProgram}\nMessages: {info.Message}\nState: {info.State}";

            Clipboard.SetText(message);
        }

        /// <summary>
        /// Handles export button click function
        /// </summary>
        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ContextMenu contextMenu = button.ContextMenu;
            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }

    }
}
