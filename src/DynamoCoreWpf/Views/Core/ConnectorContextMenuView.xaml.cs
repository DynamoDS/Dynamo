using Dynamo.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for ConnectorContextMenuView.xaml
    /// </summary>
    public partial class ConnectorContextMenuView : UserControl
    {
        public ConnectorContextMenuViewModel ViewModel { get; set; }
        public ConnectorContextMenuView()
        {
            InitializeComponent();
            this.Loaded += InitializeCommands;
        }

        private void InitializeCommands(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as ConnectorContextMenuViewModel;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.DisposeViewModel();
        }
    }
}
