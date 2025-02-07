using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Dynamo.PackageDetails
{
    public partial class PackageDetailsView : UserControl
    {
        /// <summary>
        ///     Used to notify when this control is closed 
        /// </summary>
        internal event EventHandler Closed;

        public PackageDetailsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Allows for mousewheel scrolling in the DataGrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VersionsDataGrid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled) return;
            
            e.Handled = true;

            var parentScrollViewer = this.MainScrollViewer;
            if (parentScrollViewer != null)
            {
                parentScrollViewer.ScrollToVerticalOffset(parentScrollViewer.VerticalOffset - e.Delta);
            }
         
        }

        /// <summary>
        /// The DataContext of this control is an individual PackageDetailItem
        /// Scroll up to the start of the item's info page when we assign a new PackageDetailItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameworkElement_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                MainScrollViewer.ScrollToTop();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Have to make packageManagerClientViewModel internal in order to get to the DynamoViewModel/Model/Logger
                var dataContext = this.DataContext as PackageDetailsViewModel;
                dataContext?.packageManagerClientViewModel?.DynamoViewModel?.Model.Logger.Log("Error navigating to package url: " + ex.StackTrace);
            }
            e.Handled = true;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        // Disables DataGrid interactions on mousedown (selection)
        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
    
}
