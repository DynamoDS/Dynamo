using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dynamo.Controls;
using Dynamo.UI.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for DynamoInstalledPackagesView.xaml
    /// </summary>
    public partial class InstalledPackagesView : Window
    {
        //private TitleBarButtons titleBarButtons;

        public InstalledPackagesView()
        {

            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.DataContext = dynSettings.PackageLoader;
            InitializeComponent();

            //if (titleBarButtons == null)
            //{
            //    titleBarButtons = new TitleBarButtons(this);
            //    titleBarButtonsGrid.Children.Add(titleBarButtons);
            //}

        }

        private void BrowseOnline_OnClick(object sender, RoutedEventArgs e)
        {
            dynSettings.PackageManagerClient.GoToWebsite();
        }
    }
}
