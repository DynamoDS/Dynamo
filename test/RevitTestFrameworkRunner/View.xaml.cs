using System.ComponentModel;
using System.Windows;

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

            this.Loaded += View_Loaded;
        }

        void View_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModel;
            vm.UiDispatcher = Dispatcher;
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
