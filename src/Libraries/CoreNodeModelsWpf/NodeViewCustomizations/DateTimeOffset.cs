using Dynamo.Controls;
using Dynamo.Wpf.Controls;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    public class DateTimeOffsetNodeViewCustomization : INodeViewCustomization<DSCoreNodesUI.DateTimeOffset>
    {
        public void CustomizeView(DSCoreNodesUI.DateTimeOffset model, NodeView nodeView)
        {
            var dtInputControl = new DateTimeInputControl {DataContext = model};
            nodeView.inputGrid.Children.Add(dtInputControl);
        }

        public void Dispose()
        {
        }
    }
}
