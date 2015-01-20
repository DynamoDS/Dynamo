using DSCoreNodesUI.Input;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace DSCoreNodesUI.UI
{
    public class DoubleSliderNodeViewCustomization : INodeViewCustomization<DoubleSlider>
    {
        public void CustomizeView(DoubleSlider model, NodeView nodeView)
        {
            var slider = new Dynamo.UI.Controls.DynamoSlider(model, nodeView)
            {
                DataContext = new SliderViewModel<double>(model)
            };

            nodeView.inputGrid.Children.Add(slider);
        }

        public void Dispose()
        {
        }

    }
}