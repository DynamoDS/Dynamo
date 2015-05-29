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
        }
    }
}
