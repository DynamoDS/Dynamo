using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using Dynamo.Controls;
using Dynamo.UI;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class DropDrownBaseNodeViewCustomization : INodeViewCustomization<Dynamo.Nodes.DropDrownBase>
    {
        private Nodes.DropDrownBase dropDownNodeModel;

        public void CustomizeView(Nodes.DropDrownBase nodeModel, NodeView nodeView)
        {
            dropDownNodeModel = nodeModel;

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = System.Double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            nodeView.inputGrid.Children.Add(combo);
            Grid.SetColumn(combo, 0);
            Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    dropDownNodeModel.RequiresRecalc = true;
            };

            combo.DropDownClosed += delegate
            {
                //disallow selection of nothing
                if (combo.SelectedIndex == -1)
                {
                    dropDownNodeModel.SelectedIndex = 0;
                }
            };

            combo.DataContext = nodeModel;
            //bind this combo box to the selected item hash

            var bindingVal = new Binding("Items") { Mode = BindingMode.TwoWay, Source = nodeModel };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = nodeModel
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);
        }

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItems is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void combo_DropDownOpened(object sender, EventArgs e)
        {
            dropDownNodeModel.PopulateItems();
        }

        public void Dispose()
        {
        }

    }
}