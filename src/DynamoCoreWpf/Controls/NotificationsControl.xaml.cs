using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;

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
            var window = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            if (window == null)
            {
                return;
            }

            window.PreviewMouseDown += window_PreviewMouseDown;
        }

        /// <summary>
        /// Handle the DynamoView's PreviewMouseDown event.
        /// When the user click anywhere in the view, clear the ViewModel's warning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var hsvm = (HomeWorkspaceViewModel)((DynamoViewModel)DataContext).HomeSpaceViewModel;
            // Commented this after MAGN - 8423
            // hsvm.ClearWarning();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as DynamoViewModel;
            //Open Graph Status view extension
            viewModel?.OnViewExtensionOpenRequest("3467481b-d20d-4918-a454-bf19fc5c25d7");
        }
    }
}
