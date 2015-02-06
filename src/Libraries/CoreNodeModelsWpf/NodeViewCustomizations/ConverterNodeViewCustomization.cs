using DSCoreNodesUI.Input;
using Dynamo.Controls;
using Dynamo.Wpf.Controls;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    class ConverterNodeViewCustomization : INodeViewCustomization<Convert>
    {       
        public void CustomizeView(Convert model, NodeView nodeView)
        {
            var converter = new DynamoConverterControl(model,nodeView)
            {
                DataContext = new SliderViewModel<double>(model)
            }
        }
    }
}
