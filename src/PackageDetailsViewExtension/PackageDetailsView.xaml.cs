using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PackageDetails
{
    public partial class PackageDetailsView : UserControl
    {
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
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }
    }
}
