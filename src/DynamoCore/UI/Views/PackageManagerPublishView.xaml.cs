using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.UI.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishView.xaml
    /// </summary>
    public partial class PackageManagerPublishView : Window
    {
        //private TitleBarButtons titleBarButtons;

        public PackageManagerPublishView(PublishPackageViewModel packageViewModel)
        {

            this.DataContext = packageViewModel;
            packageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;

            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();

            //if (titleBarButtons == null)
            //{
            //    titleBarButtons = new TitleBarButtons(this);
            //    titleBarButtonsGrid.Children.Add(titleBarButtons);
            //}

        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            this.Dispatcher.BeginInvoke((Action) (Close));
        }
    }

}
