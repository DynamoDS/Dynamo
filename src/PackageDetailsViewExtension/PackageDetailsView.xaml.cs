using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PackageDetails
{
    public partial class PackageDetailsView : UserControl
    {
        public PackageDetailsViewModel PackageDetailsViewModel { get; }
        public PackageDetailsView(PackageDetailsViewModel packageDetailsViewModel)
        {
            InitializeComponent();
            this.PackageDetailsViewModel = packageDetailsViewModel;
            DataContext = PackageDetailsViewModel;
        }

        private void VersionsDataGrid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }
    }
}
