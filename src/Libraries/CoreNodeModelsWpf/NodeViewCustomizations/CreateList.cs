using CoreNodeModels;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    public class CreateListNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<CreateList>
    {
        public void CustomizeView(CreateList model, Dynamo.Controls.NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }
}
