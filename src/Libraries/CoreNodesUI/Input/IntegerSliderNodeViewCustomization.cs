using System.Globalization;

using DSCoreNodesUI;

using Dynamo.Controls;
using Dynamo.Wpf;

namespace Dynamo.Nodes
{
    public class IntegerSliderNodeViewCustomization : INodeViewCustomization<IntegerSlider>
    {
        public void CustomizeView(IntegerSlider model, dynNodeView nodeUI)
        {
            DoubleSliderNodeViewCustomization.BuildSliderUI(
                nodeUI, 
                model, 
                model.Value, 
                model.Value.ToString(CultureInfo.InvariantCulture),
                new IntegerSliderSettingsControl() { DataContext = model }, 
                new IntegerDisplay());
        }

        public void Dispose() { }
    }
}