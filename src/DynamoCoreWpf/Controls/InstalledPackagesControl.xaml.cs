using Dynamo.ViewModels;
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

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for InstalledPackagesControl.xaml
    /// </summary>
    public partial class InstalledPackagesControl : UserControl
    {
        public InstalledPackagesControl()
        {
            InitializeComponent();
        }
        private void MoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.IsOpen = true;
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander exp = (Expander)sender;
            PackageViewModel pm = (PackageViewModel)exp.DataContext;
            pm.ToggleTypesVisibleInManagerCommand.Execute();
        }
    }
}
