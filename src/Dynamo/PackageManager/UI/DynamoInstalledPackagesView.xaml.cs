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

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for DynamoInstalledPackagesView.xaml
    /// </summary>
    public partial class DynamoInstalledPackagesView : Window
    {
        public DynamoInstalledPackagesView( PackageManagerClient client )
        {
            this.DataContext = client;
            InitializeComponent();
        }
    }
}
