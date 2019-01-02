using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.UI;
using Dynamo.ViewModels;
using DynamoUtilities;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

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
            viewModel.RequestShowFileDialog += OnRequestShowFileDialog;
            viewModel.PropertyChanged += OnPropertyChanged;
            UpdateVisualToReflectSelectionState();
            PreviewKeyDown += OnPackagePathDialogKeyDown;
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

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedIndex"))
            {
                // Repositioning the selection should retain its visual state.
                UpdateVisualToReflectSelectionState();
            }
        }

        private void OnEllipsisClicked(object sender, MouseButtonEventArgs e)
        {
            var selectedIndex = ViewModel.SelectedIndex;
            if (ViewModel.UpdatePathCommand.CanExecute(selectedIndex))
                ViewModel.UpdatePathCommand.Execute(selectedIndex);
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SaveSettingCommand.CanExecute(null))
            {
                ViewModel.SaveSettingCommand.Execute(null);
                this.Close(); // Close the dialog after saving.
            }
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the dialog without saving.
        }

        private void OnRequestShowFileDialog(object sender, EventArgs e)
        {
            var args = e as PackagePathEventArgs;
            args.Cancel = true;

            // Handle for the case, args.Path does not exist.
            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(args.Path);
            // args.Path == null condition is to handle when user want to create new path.
            if (errorCannotCreateFolder == null || args.Path == null)
            {
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
            else
            {
                string errorMessage = string.Format(Wpf.Properties.Resources.PackageFolderNotAccessible, args.Path);
                System.Windows.Forms.MessageBox.Show(errorMessage, Wpf.Properties.Resources.UnableToAccessPackageDirectory, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateVisualToReflectSelectionState()
        {
            var newIndex = ViewModel.SelectedIndex;
            if (PathListBox.SelectedIndex != newIndex)
                PathListBox.SelectedIndex = newIndex;
        }

        #endregion

        private void OnPackagePathDialogKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Return)
            {
                ViewModel.SaveSettingCommand.Execute(null);
                e.Handled = true;
                Close();
            }
        }
    }
}
