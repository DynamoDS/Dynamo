using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for NotificationsControl.xaml
    /// </summary>
    public partial class NotificationsControl : UserControl
    {
        public NotificationsControl()
        {
            InitializeComponent();

            this.Loaded += NotificationsControl_Loaded;  
        }

        void NotificationsControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Dynamo.Utilities.WpfUtilities.FindUpVisualTree<DynamoView>(this);
            window.PreviewMouseDown += window_PreviewMouseDown;
        }

        /// <summary>
        /// Handle the DynamoView's PreviewMouseDown event.
        /// When the user click anywhere in the view, clear the ViewModel's warning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = (NotificationsViewModel)DataContext;
            vm.ClearWarning();
        }
    }
}
