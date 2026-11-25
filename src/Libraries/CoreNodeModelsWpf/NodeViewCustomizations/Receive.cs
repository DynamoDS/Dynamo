using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// Customizes the WPF view for the <see cref="Receive"/> node with an inline key/wildcard editor.
    /// </summary>
    public class ReceiveNodeViewCustomization : INodeViewCustomization<Receive>
    {
        /// <inheritdoc />
        public void CustomizeView(Receive model, NodeView nodeView)
        {
            var textBox = new DynamoTextBox
            {
                MinWidth = 140,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };

            nodeView.inputGrid.Children.Add(textBox);
            Grid.SetColumn(textBox, 0);
            Grid.SetRow(textBox, 0);

            textBox.DataContext = model;
            textBox.BindToProperty(new Binding(nameof(Receive.ChannelName))
            {
                Mode = BindingMode.TwoWay,
                Source = model,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
