using CoreNodeModelsWpf.Controls;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Wpf;
using Dynamo.Wpf.Controls;

namespace CoreNodeModelsWpf.NodeViewCustomizations
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
