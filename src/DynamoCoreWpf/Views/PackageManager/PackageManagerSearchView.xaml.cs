﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.PackageManager.ViewModels;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchView.xaml
    /// </summary>
    public partial class PackageManagerSearchView : Window
    {
        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {
            this.DataContext = pm;
            InitializeComponent();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults(this.SearchTextBox.Text);
        }

        public void ItemStackPanel_MouseDown(object sender, RoutedEventArgs e)
        {
            var lbi = sender as StackPanel;
            if (lbi == null) return;

            var viewModel = lbi.DataContext as PackageManagerSearchElementViewModel;
            if (viewModel == null) return;

            viewModel.Model.IsExpanded = !viewModel.Model.IsExpanded;
        }

        private void OnShowContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnSortButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

        private void OnInstallLatestButtonDropDownClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

        private void OnInstallVersionButtonDropDownClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

    }
}
