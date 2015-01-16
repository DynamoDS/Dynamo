using System.Globalization;
using Dynamo.Nodes;
using Dynamo.Wpf;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Wpf
{
    public class IntegerSliderNodeViewCustomization : INodeViewCustomization<IntegerSlider>
    {
        public void CustomizeView(IntegerSlider model, NodeView nodeView)
        {
            DoubleSliderNodeViewCustomization.BuildSliderUI(
                nodeView, 
                model, 
                model.Value, 
                model.Value.ToString(CultureInfo.InvariantCulture),
                new IntegerSliderSettingsControl() { DataContext = model }, 
                new IntegerDisplay());
        }

        public void Dispose() { }
    }
}