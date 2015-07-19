using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Views.PackageManager
{
    /// <summary>
    /// Interaction logic for PackagePathView.xaml
    /// </summary>
    public partial class PackagePathView : Window
    {
        public PackagePathView()
        {
            InitializeComponent();
        }

        internal PackagePathView(PackagePathViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            InitializeComponent();
            this.DataContext = viewModel;
        }

        #region Private Helper Methods and Event Handlers

        void OnPathSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return; // Nothing selected.

            var viewModel = DataContext as PackagePathViewModel;
            var selected = e.AddedItems[0] as string;
            viewModel.SelectedIndex = viewModel.RootLocations.IndexOf(selected);
        }

        private void OnEllipsisClicked(object sender, MouseButtonEventArgs e)
        {
            var clicked = sender as TextBlock;
            var dataString = clicked.DataContext as string;

            var viewModel = DataContext as PackagePathViewModel;
            BrowseForFolder(viewModel.RootLocations.IndexOf(dataString));
        }

        private void BrowseForFolder(int replacement)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var selected = dialog.SelectedPath;
            var viewModel = DataContext as PackagePathViewModel;

            if (replacement != -1)
                viewModel.RootLocations[replacement] = selected;
            else
                viewModel.RootLocations.Add(selected);
        }

        #endregion
    }
}
