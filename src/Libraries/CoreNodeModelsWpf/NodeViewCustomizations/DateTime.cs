using Dynamo.Controls;
using Dynamo.Wpf.Controls;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    public class DateTimeNodeViewCustomization : INodeViewCustomization<DSCoreNodesUI.DateTime>
    {
        public void CustomizeView(DSCoreNodesUI.DateTime model, NodeView nodeView)
        {
            var dtInputControl = new DateTimeInputControl {DataContext = model};
            nodeView.inputGrid.Children.Add(dtInputControl);
        }

        public void Dispose()
        {
        }
    }
}
