using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Wpf;
using Dynamo.Wpf.Controls;

namespace CoreNodeModelsWpf.NodeViewCustomizations
{
    public class IntegerSliderNodeViewCustomization : INodeViewCustomization<IntegerSlider>
    {
        public void CustomizeView(IntegerSlider model, NodeView nodeView)
        {
            var slider = new DynamoSlider(model, nodeView)
            {
                DataContext = new SliderViewModel<int>(model)
            };

            nodeView.inputGrid.Children.Add(slider);
        }

        public void Dispose() { }
    }
}