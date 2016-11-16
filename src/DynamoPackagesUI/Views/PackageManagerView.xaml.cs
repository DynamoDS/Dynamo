using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DynamoPackagesUI.Views
{
    /// <summary>
    /// Interaction logic for PackageManagerView.xaml
    /// </summary>
    public partial class PackageManagerView : Window
    {
        public PackageManagerView()
        {
            InitializeComponent();

            this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.95);
            this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.75);
        }
    }
}
