using Autodesk.DesignScript.Runtime;

using DSCoreNodesUI.Input;

using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName("Number Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces numeric values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    [NodeSearchTags(new[] { "double", "number", "float", "integer", "slider" })]
    public class DoubleSlider : SliderBase
    {
        public DoubleSlider(WorkspaceModel workspace)
            : base(workspace)
        {
            Min = 0;
            Max = 100;
            Step = 0.01;
            Value = 0;
        }
    }
}