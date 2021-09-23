using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using Dynamo.Controls;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.NodeViewCustomizations
{
    public class DoubleSliderNodeViewCustomization : INodeViewCustomization<DoubleSlider>
    {
        private NodeView viewNode;
        private DoubleSlider sliderModel;
        private DynamoSlider sliderDynamo;
        public void CustomizeView(DoubleSlider model, NodeView nodeView)
        {
            viewNode = nodeView;
            sliderModel = model;
            viewNode.MouseEnter += ViewNode_MouseEnter;
        }

        private void ViewNode_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ConstructView();
        }

        private void ConstructView()
        {
            if (viewNode is null || sliderModel is null || sliderDynamo!= null)
            {
                return;
            }

            sliderDynamo = new DynamoSlider(sliderModel, viewNode)
            {
                DataContext = new SliderViewModel<double>(sliderModel)
            };

            viewNode.inputGrid.Children.Add(sliderDynamo);
        }

        public void Dispose()
        {
            viewNode.MouseEnter -= ViewNode_MouseEnter;
        }
    }
}