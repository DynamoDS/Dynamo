using Dynamo.PackageManager;
using Dynamo.PackageManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchControl.xaml
    /// </summary>
    public partial class PackageManagerSearchControl : UserControl
    {
        public PackageManagerSearchViewModel PkgSearchVM { get; set; }

        public PackageManagerSearchControl()
        {
            InitializeComponent();

            this.Loaded += InitializeContext;
        }

        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            PkgSearchVM = this.DataContext as PackageManagerSearchViewModel;
        }


        private void OnShowFilterContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnFilterButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowFilterContextMenuFromLeftClicked(sender, e);
        }


        private void OnShowContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnSortButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

      

        /// <summary>
        /// Executes a command that opens the package details view extension.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            PkgSearchVM.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.Model);
        }
    }
}
