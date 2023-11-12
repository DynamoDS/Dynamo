using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.ViewModels;
using System;
using CoreNodeModels;

namespace Dynamo.Controls
{
    //http://blogs.msdn.com/b/chkoenig/archive/2008/05/24/hierarchical-databinding-in-wpf.aspx

    /// <summary>
    /// Interaction logic for WatchTree.xaml
    /// </summary>
    public partial class WatchTree : UserControl
    {
        private WatchViewModel _vm;
        private WatchViewModel prevWatchViewModel;
        private static readonly double defaultWidthSize = 200;
        private readonly double extraWidthSize = 20;
        private readonly double widthPerCharacter = 7.5;
        private static readonly int defaultHeightSize = 200;
        private readonly int minWidthSize = 100;
        private readonly int minHeightSize = 38;
        private readonly int minHeightForList = 83;

        public WatchTree(WatchViewModel vm)
        {
            _vm = vm;

            InitializeComponent();

            DataContext = vm;
            this.Loaded += WatchTree_Loaded;
            this.Unloaded += WatchTree_Unloaded;
        }

        internal static double DefaultWidthSize { get { return defaultWidthSize; } }
        internal static double DefaultHeightSize { get { return defaultHeightSize; } }
        internal double ExtratWidthSize { get { return extraWidthSize; } }
        internal double WidthPerCharacter { get { return widthPerCharacter; } }
        internal double MaxWidthSize { get { return defaultWidthSize * 2; } }
        internal string NodeLabel { get { return _vm.Children[0].NodeLabel; } }

        private void WatchTree_Unloaded(object sender, RoutedEventArgs e)
        {
            _vm.PropertyChanged -= _vm_PropertyChanged;
        }

        void WatchTree_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.PropertyChanged += _vm_PropertyChanged;
        }

        private void _vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //We need to restore the custom size for the watch nodes in the workspace. 
            var customSizesDict = Watch.NodeSizes;
            var customWidth = double.NaN;
            var customHeight= double.NaN;

            if (_vm.WatchNode != null)
            {
                customSizesDict.TryGetValue(_vm.WatchNode.GUID, out var customSizes);
                customWidth = customSizes == null ? double.NaN : customSizes.Item1;
                customHeight = customSizes == null ? double.NaN : customSizes.Item2;
            }

            if (e.PropertyName == nameof(WatchViewModel.IsCollection))
            {
                // // The WatchTree controll will resize only if its role is a WatchNode (starts with an specific height), otherwise it won't resize (Bubble role).
                if (!Double.IsNaN(this.Height))
                {
                    if (_vm.IsCollection)
                    {
                        Height = this.Height != customHeight ? defaultHeightSize : Height;
                        inputGrid.MinHeight = minHeightForList;
                    }
                    else
                    {
                        this.Height = minHeightSize;
                        if (_vm.Children.Count != 0)
                        {
                            if (NodeLabel.Contains(Environment.NewLine) || NodeLabel.ToUpper() == nameof(WatchViewModel.DICTIONARY))
                            {
                                Height = this.Height != customHeight ? defaultHeightSize : Height;
                            }
                        }
                    }
                    // When it doesn't have any element, it should be set back the width to the default.
                    if (_vm.Children != null && _vm.Children.Count == 0)
                    {
                        this.Width = defaultWidthSize;
                    }
                }
            }
            else if (e.PropertyName == nameof(WatchViewModel.Children))
            {
                if (_vm.Children != null)
                {
                    if (!_vm.Children[0].IsCollection)
                    {
                        // We will use 7.5 as width factor for each character.

                        double requiredWidth = (NodeLabel.Length * widthPerCharacter);
                        if (requiredWidth > (MaxWidthSize))
                        {
                            requiredWidth = MaxWidthSize;
                        }
                        requiredWidth += extraWidthSize;
                        this.Width = requiredWidth;
                    }
                    else
                    {
                        this.Width = this.Width != customWidth ? defaultWidthSize : this.Width;
                    }
                }
            }
            else if (e.PropertyName == nameof(WatchViewModel.IsOneRowContent))
            {
                if (_vm.IsOneRowContent)
                {
                    // Forcing not to display the Levels content when is being used for display info from another node like the Color Range
                    this.ListLevelsDisplay.Visibility = Visibility.Hidden;
                    this.ListLevelsDisplay.Height = 0;
                }
            }
        }

        internal void SetWatchNodeProperties() 
        {
            resizeThumb.Visibility = Visibility.Visible;
            this.Width = defaultWidthSize;
            this.Height = minHeightSize;
            inputGrid.MinHeight = minHeightSize;
            inputGrid.MinWidth = minWidthSize;
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
            if (prevWatchViewModel != null)
            {
                if (e.Key == System.Windows.Input.Key.Up || e.Key == System.Windows.Input.Key.Down)
                {
                    TreeViewItem tvi = sender as TreeViewItem;
                    var node = tvi.DataContext as WatchViewModel;
                    
                    // checks to see if the currently selected WatchViewModel is the top most item in the tree
                    // if so, prevent the user from using the up arrow.

                    // also if the current selected node is equal to the previous selected node, do not execute the change.

                    if (!(e.Key == System.Windows.Input.Key.Up && prevWatchViewModel.IsTopLevel) && node != prevWatchViewModel)
                    {
                        HandleItemChanged(tvi, node);
                    }
                }
            }

            e.Handled = true;
        }

        private void HandleItemChanged (TreeViewItem tvi, WatchViewModel node)
        {
            if (tvi == null || node == null)
                return;

            // checks to see if the node to be selected is the same as the currently selected node
            // if so, then de-select the currently selected node.

            if (node == prevWatchViewModel)
            {
                this.prevWatchViewModel = null;
                if (tvi.IsSelected)
                {
                    tvi.IsSelected = false;
                    tvi.Focus();
                }
            }
            else
            {
                this.prevWatchViewModel = node;
            }

            if (_vm.FindNodeForPathCommand.CanExecute(node.Path))
            {
                _vm.FindNodeForPathCommand.Execute(node.Path);
            }
        }
        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (xAdjust >= inputGrid.MinWidth)
            {
                Width = xAdjust;
            }

            if (yAdjust >= inputGrid.MinHeight)
            {
                Height = yAdjust;
            }

            Watch.NodeSizes[_vm.WatchNode.GUID] = new Tuple<double, double>(Width, Height);
        }
    }
}
