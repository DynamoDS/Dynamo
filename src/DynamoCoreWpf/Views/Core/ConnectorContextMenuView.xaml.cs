using Dynamo.ViewModels;
using System;
using System.Reflection;
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
            this.Loaded -= InitializeCommands;
            ViewModel = this.DataContext as ConnectorContextMenuViewModel;
            MainContextMenu.DataContext = ViewModel;
            MainContextMenu.IsOpen = true;
        }
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            this.MouseLeave -= OnMouseLeave;
            ViewModel.RequestDisposeViewModel();
        }
        private void OnContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            MainContextMenu.ContextMenuClosing -= OnContextMenuClosing;
            ViewModel.RequestDisposeViewModel();
        }

        private void OnMouseLeaveContextMenu(object sender, MouseEventArgs e)
        {
            MainContextMenu.MouseLeave -= OnMouseLeave;
            ViewModel.RequestDisposeViewModel();
        }
    }
}
