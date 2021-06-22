using System;
using System.ComponentModel;
using System.Linq;
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
    public partial class PackagePathView : System.Windows.Controls.UserControl
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
            DataContextChanged += PackagePathView_DataContextChanged;
        }

        internal PackagePathView(PackagePathViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException("viewModel");

            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.RequestShowFileDialog += OnRequestShowFileDialog;
        }

        #endregion

        #region Private Helper Methods and Event Handlers
        private void PackagePathView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.RequestShowFileDialog += OnRequestShowFileDialog;
            }
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
                    Owner = System.Windows.Application.Current.Windows.OfType<PreferencesView>().SingleOrDefault(x => x.IsActive)
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
        #endregion
    }
}
