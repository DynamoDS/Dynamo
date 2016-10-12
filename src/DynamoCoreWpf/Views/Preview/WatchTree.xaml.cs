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
        private WatchViewModel prevVM;

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

        private void treeviewItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
            var node = tvi.DataContext as WatchViewModel;

            HandleItemChanged(tvi, node);

            e.Handled = true; 
        }
        private void treeviewItem_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up || e.Key == System.Windows.Input.Key.Down)
            {
                TreeViewItem tvi = sender as TreeViewItem;
                var node = tvi.DataContext as WatchViewModel;

                HandleItemChanged(tvi, node);
            }
            e.Handled = true;
        }

        private void HandleItemChanged (TreeViewItem tvi, WatchViewModel node)
        {
            if (tvi == null || node == null)
                return;

            // checks to see if the currently selected node is the same as the previous selected node
            // if so, then de-select the currently selected node.

            if (prevVM == null || node.Path != this.prevVM.Path)
            {
                this.prevVM = node;
            }
            else
            {
                this.prevVM = null;
                if (tvi.IsSelected)
                {
                    tvi.IsSelected = false;
                    tvi.Focus();
                }
            }

            if (_vm.FindNodeForPathCommand.CanExecute(node.Path))
            {
                _vm.FindNodeForPathCommand.Execute(node.Path);
            }
        }
    }
}
