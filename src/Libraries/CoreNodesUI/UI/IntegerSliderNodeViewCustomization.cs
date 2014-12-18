using DSCoreNodesUI.Input;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    public class IntegerSliderNodeViewCustomization : INodeViewCustomization<IntegerSlider>
    {
        public void CustomizeView(IntegerSlider model, NodeView nodeView)
        {
            var slider = new UI.Controls.DynamoSlider(model, nodeView)
            {
                DataContext = new SliderViewModel(NumericFormat.Integer, model)
            };

            nodeView.inputGrid.Children.Add(slider);
        }

        public void Dispose() { }
    }
}