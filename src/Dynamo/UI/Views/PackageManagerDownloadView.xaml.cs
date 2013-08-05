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
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerDownloadView.xaml
    /// </summary>
    public partial class PackageManagerDownloadView : Window
    {
        public PackageManagerDownloadView()
        {

            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            InitializeComponent();
        }

        private void Clear_Completed_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
