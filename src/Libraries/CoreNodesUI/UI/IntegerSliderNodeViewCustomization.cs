using DSCoreNodesUI.Input;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace DSCoreNodesUI.UI
{
    public class IntegerSliderNodeViewCustomization : INodeViewCustomization<IntegerSlider>
    {
        public void CustomizeView(IntegerSlider model, NodeView nodeView)
        {
            var slider = new Dynamo.UI.Controls.DynamoSlider(model, nodeView)
            {
                DataContext = new SliderViewModel<int>(model)
            };

            nodeView.inputGrid.Children.Add(slider);
        }

        public void Dispose() { }
    }
}