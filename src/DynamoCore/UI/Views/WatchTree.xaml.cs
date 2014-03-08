using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

//using Autodesk.Revit.DB;

namespace Dynamo.Controls
{
    //http://blogs.msdn.com/b/chkoenig/archive/2008/05/24/hierarchical-databinding-in-wpf.aspx

    /// <summary>
    /// Interaction logic for WatchTree.xaml
    /// </summary>
    public partial class WatchTree : UserControl
    {
        public WatchTree()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //find the element which was clicked
            //and implement it's method for jumping to stuff
            var fe = sender as FrameworkElement;

            if (fe == null)
                return;

            var node = (WatchItem)fe.DataContext;

            if (node != null)
                node.Click();
        }

        private void TreeView1_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //var node = e.NewValue as WatchItem;

            //if (node == null)
            //    return;

            //var package =
            //    dynSettings.Controller.DynamoModel.Nodes.SelectMany(x => x.RenderPackages)
            //        .FirstOrDefault(x => x.Tag == node.Path);

            //if (package != null)
            //{
            //    Debug.WriteLine("Found render package for selection: " + node.Path);
            //}
        }
    }
}
