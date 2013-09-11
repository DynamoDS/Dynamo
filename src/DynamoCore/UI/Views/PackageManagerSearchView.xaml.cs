using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchView.xaml
    /// </summary>
    public partial class PackageManagerSearchView : Window
    {
        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {

            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.DataContext = pm;
            InitializeComponent();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults(((TextBox)sender).Text);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((PackageManagerSearchViewModel)DataContext).ExecuteSelected();
        }

    }
}
