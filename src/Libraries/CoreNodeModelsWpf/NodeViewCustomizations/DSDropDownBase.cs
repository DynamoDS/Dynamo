using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    public class DropDownNodeViewCustomization : INodeViewCustomization<DSDropDownBase>
    {
        private DSDropDownBase model;

        public void CustomizeView(DSDropDownBase model, NodeView nodeView)
        {
            this.model = model;

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
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    model.OnNodeModified();
            };

            combo.DataContext = model;

            // bind this combo box to the selected item hash
            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Mode = BindingMode.TwoWay,
                Source = model
            };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            // bind the selected index to the model property SelectedIndex
            var indexBinding = new Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = model
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItems is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            this.model.PopulateItems();
        }

    }
}