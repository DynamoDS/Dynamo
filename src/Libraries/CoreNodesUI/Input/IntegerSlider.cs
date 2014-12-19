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

        protected override bool UpdateValueCore(string name, string value)
        {
            switch (name)
            {
                case "Min":
                case "MinText":
                    Min = (int)SliderViewModel.ConvertToDouble(NumericFormat.Integer, value);
                    return true; // UpdateValueCore handled.
                case "Max":
                case "MaxText":
                    Max = (int)SliderViewModel.ConvertToDouble(NumericFormat.Integer, value);
                    return true; // UpdateValueCore handled.
                case "Value":
                case "ValueText":
                    Value = (int)SliderViewModel.ConvertToDouble(NumericFormat.Integer, value);
                    if (Value >= Max)
                    {
                        this.Max = Value;
                    }
                    if (Value <= Min)
                    {
                        this.Min = Value;
                    }
                    return true; // UpdateValueCore handled.
                case "Step":
                case "StepText":
                    Step = SliderViewModel.ConvertToDouble(NumericFormat.Double, value);
                    return true;
            }

            return base.UpdateValueCore(name, value);
        }
    }
}
