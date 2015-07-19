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
        #region Class Properties

        private PackagePathViewModel ViewModel
        {
            get { return this.DataContext as PackagePathViewModel; }
        }

        #endregion

        #region Public Class Operational Methods

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

        #endregion

        #region Private Helper Methods and Event Handlers

        void OnPathSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return; // Nothing selected.

            var selected = e.AddedItems[0] as string;
            ViewModel.SelectedIndex = ViewModel.RootLocations.IndexOf(selected);
        }

        private void OnEllipsisClicked(object sender, MouseButtonEventArgs e)
        {
            var clicked = sender as TextBlock;
            var dataString = clicked.DataContext as string;
            BrowseForFolder(ViewModel.RootLocations.IndexOf(dataString));
        }

        private void BrowseForFolder(int replacement)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var selected = dialog.SelectedPath;
            if (replacement != -1)
                ViewModel.RootLocations[replacement] = selected;
            else
                ViewModel.RootLocations.Add(selected);
        }

        #endregion
    }
}
