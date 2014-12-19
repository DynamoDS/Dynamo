using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using DSCoreNodesUI;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    public class DoubleSliderNodeViewCustomization : INodeViewCustomization<DoubleSlider>
    {
        public void CustomizeView(DoubleSlider model, NodeView nodeView)
        {
            BuildSliderUI(nodeView, model, model.Value, model.Value.ToString(CultureInfo.InvariantCulture),
                          new DoubleSliderSettingsControl() { DataContext = model }, new DoubleDisplay());
        }

        public void Dispose()
        {
        }

        internal static void BuildSliderUI(NodeView nodeUI, NodeModel nodeModel,
                                           double value, string serializedValue, UIElement sliderSettingsControl,
                                           IValueConverter numberDisplayConverter)
        {
            //add a slider control to the input grid of the control
            var tbSlider = new DynamoSlider(nodeModel)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = Configurations.PortHeightInPixels,
                MinWidth = 150,
                TickPlacement = TickPlacement.None,
                Value = value
            };

            tbSlider.PreviewMouseUp += delegate
            {
                nodeUI.ViewModel.DynamoViewModel.ReturnFocusToSearch();
            };

            // build grid for input and expander
            var textBoxExpanderGrid = new Grid()
            {
                MinWidth = 150
            };

            textBoxExpanderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            textBoxExpanderGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(29)
            });

            // input value textbox
            var valtb = new DynamoTextBox(serializedValue);

            Grid.SetColumn(valtb, 0);
            textBoxExpanderGrid.Children.Add(valtb);

            var exp = new Expander();
            exp.Padding = new System.Windows.Thickness(4, 0, 0, 0);
            Grid.SetColumn(exp, 1);
            textBoxExpanderGrid.Children.Add(exp);

            var sliderSp = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            sliderSp.Children.Add(textBoxExpanderGrid);

            // min max control
            var minMaxControl = sliderSettingsControl;
            minMaxControl.Visibility = Visibility.Collapsed;
            sliderSp.Children.Add(minMaxControl);

            // expander modifies visibility of min/max
            exp.Expanded += (sender, args) =>
            {
                minMaxControl.Visibility = Visibility.Visible;
            };

            exp.Collapsed += (sender, args) =>
            {
                minMaxControl.Visibility = Visibility.Collapsed;
            };

            nodeUI.inputGrid.Children.Add(tbSlider);
            nodeUI.PresentationGrid.Children.Add(sliderSp);
            nodeUI.PresentationGrid.Visibility = Visibility.Visible;

            tbSlider.DataContext = nodeModel;
            valtb.DataContext = nodeModel;

            // value input
            valtb.BindToProperty(
                new Binding("Value") { Mode = BindingMode.TwoWay, Converter = numberDisplayConverter });

            // slider value 
            var sliderBinding = new Binding("Value") { Mode = BindingMode.TwoWay, Source = nodeModel };
            tbSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);

            // max slider value
            var bindingMaxSlider = new Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MaximumProperty, bindingMaxSlider);

            // min slider value
            var bindingMinSlider = new Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = nodeModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tbSlider.SetBinding(RangeBase.MinimumProperty, bindingMinSlider);
        }

    }
}