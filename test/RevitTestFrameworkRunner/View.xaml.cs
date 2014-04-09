using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitTestFrameworkRunner
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : Window
    {
        public View(ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            this.Closing += View_Closing;
        }

        private void View_Closing(object sender, CancelEventArgs e)
        {
            this.DialogResult = true;
        }

        private void TestDataTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = DataContext as ViewModel;
            vm.SelectedItem = e.NewValue;
        }
    }
}
