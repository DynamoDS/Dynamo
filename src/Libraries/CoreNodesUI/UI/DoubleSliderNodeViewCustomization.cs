using DSCoreNodesUI.Input;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    public class DoubleSliderNodeViewCustomization : INodeViewCustomization<DoubleSlider>
    {
        public void CustomizeView(DoubleSlider model, NodeView nodeView)
        {
            var slider = new UI.Controls.DynamoSlider(model, nodeView)
            {
                DataContext = new SliderViewModel(NumericFormat.Double, model)
            };

            nodeView.inputGrid.Children.Add(slider);
        }

        public void Dispose()
        {
        }

    }
}