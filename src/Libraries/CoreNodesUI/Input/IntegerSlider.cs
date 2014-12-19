using Autodesk.DesignScript.Runtime;

using DSCoreNodesUI.Input;

using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName("Integer Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A slider that produces integer values.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public class IntegerSlider : SliderBase
    {
        public IntegerSlider(WorkspaceModel workspace)
            : base(workspace)
        {
            RegisterAllPorts();

            Min = 0;
            Max = 100;
            Step = 1;
            Value = 0;
        }
    }
}
