using DSCoreNodesUI.Input;
using Dynamo.Controls;
using Dynamo.Wpf.Controls;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    public class DateTimeNodeViewCustomization : INodeViewCustomization<DateTime>
    {
        public void CustomizeView(DateTime model, NodeView nodeView)
        {
            var dtInputControl = new DateTimeInputControl {DataContext = model};
            nodeView.inputGrid.Children.Add(dtInputControl);
        }

        public void Dispose()
        {
        }
    }
}
