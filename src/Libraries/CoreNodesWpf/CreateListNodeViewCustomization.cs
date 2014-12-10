using Dynamo.Wpf;

namespace DSCoreNodesUI
{
    public class CreateListNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<CreateList>
    {
        public void CustomizeView(CreateList model, Dynamo.Controls.NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }
}