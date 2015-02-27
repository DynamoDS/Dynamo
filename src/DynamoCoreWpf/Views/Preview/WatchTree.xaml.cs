using System.Windows;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    //http://blogs.msdn.com/b/chkoenig/archive/2008/05/24/hierarchical-databinding-in-wpf.aspx

    /// <summary>
    /// Interaction logic for WatchTree.xaml
    /// </summary>
    public partial class WatchTree : UserControl
    {
        private WatchViewModel _vm;

        public WatchTree()
        {
            InitializeComponent();
            this.Loaded += WatchTree_Loaded;
        }

        void WatchTree_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = this.DataContext as WatchViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //find the element which was clicked
            //and implement it's method for jumping to stuff
            var fe = sender as FrameworkElement;

            if (fe == null)
                return;

            var node = (WatchViewModel)fe.DataContext;

            if (node != null)
                node.Click();
        }

        private void TreeView1_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as WatchViewModel;
            if (node == null)
                return;

            if (_vm.FindNodeForPathCommand.CanExecute(node.Path))
            {
                _vm.FindNodeForPathCommand.Execute(node.Path);
            }
        }
    }
}
