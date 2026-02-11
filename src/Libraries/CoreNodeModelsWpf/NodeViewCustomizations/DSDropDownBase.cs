using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.UI;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    public class DropDownNodeViewCustomization : INodeViewCustomization<DSDropDownBase>
    {
        private DSDropDownBase model;
        private ComboBox comboBox;

        public void CustomizeView(DSDropDownBase model, NodeView nodeView)
        {
            this.model = model;

            // Add a drop down list to the window
            comboBox = new ComboBox
            {
                Width = double.NaN,
                MinWidth = 150,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)SharedDictionaryManager.DynamoModernDictionary["RefreshComboBox"]
            };

            nodeView.inputGrid.Children.Add(comboBox);
            Grid.SetColumn(comboBox, 0);
            Grid.SetRow(comboBox, 0);

            comboBox.DropDownOpened += DropDownOpened;

            comboBox.DataContext = model;

            // Bind this combo box to the selected item hash.
            var bindingVal = new Binding(nameof(DSDropDownBase.Items))
            {
                Mode = BindingMode.TwoWay,
                Source = model
            };
            comboBox.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);
            
            // Bind the selected index to the model property SelectedIndex.
            var indexBinding = new Binding(nameof(DSDropDownBase.SelectedIndex))
            {
                Mode = BindingMode.TwoWay,
                Source = model
            };
            comboBox.SetBinding(Selector.SelectedIndexProperty, indexBinding);

            comboBox.SelectionChanged += SelectionChanged;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedIndex != -1)
            {
                model.OnNodeModified();
            }
        }

        public void Dispose()
        {
            comboBox.DropDownOpened -= DropDownOpened;
            comboBox.SelectionChanged -= SelectionChanged;
        }

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItems is called
        /// </summary>
        void DropDownOpened(object sender, EventArgs e)
        {
            model.PopulateItems();
        }

    }
}
