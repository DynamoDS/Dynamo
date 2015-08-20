using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.PackageManager.ViewModels;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchView.xaml
    /// </summary>
    public partial class PackageManagerSearchView : Window
    {
        private PackageManagerSearchViewModel PackageManagerSearchViewModel;

        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {
            this.DataContext = pm;
            this.PackageManagerSearchViewModel = pm;
            InitializeComponent();

            pm.RequestShowFileDialog += OnRequestShowFileDialog_SearchList;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            PackageManagerSearchViewModel.RequestShowFileDialog -= OnRequestShowFileDialog_SearchList;
            base.OnClosing(e);
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

        private void ShowContextMenuFromLeftClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void SortButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowContextMenuFromLeftClick(sender, e);
        }

        private void InstallLatestButtonDropDown_OnClick(object sender, RoutedEventArgs e)
        {
            ShowContextMenuFromLeftClick(sender, e);
        }

        private void InstallVersionButtonDropDown_OnClick(object sender, RoutedEventArgs e)
        {
            ShowContextMenuFromLeftClick(sender, e);
        }

        private void OnRequestShowFileDialog_SearchList(object sender, EventArgs e)
        {
            var args = e as PackagePathEventArgs;
            args.Cancel = true;

            var dialog = new DynamoFolderBrowserDialog
            {
                // Navigate to initial folder.
                SelectedPath = args.Path,
                Owner = this
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                args.Cancel = false;
                args.Path = dialog.SelectedPath;
            }
        }

    }
}
