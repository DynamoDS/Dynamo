using System.Collections.Generic;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using CoreNodeModels.Properties;

namespace CoreNodeModels.Input
{
    public abstract class Bool : BasicInteractive<bool>
    {
        protected override bool DeserializeValue(string val)
        {
            try
            {
                return val.ToLower().Equals("true");
            }
            catch
            {
                return false;
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            if (updateValueParams.PropertyName == "Value")
            {
                Value = DeserializeValue(updateValueParams.PropertyValue);
                return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        protected override string SerializeValue()
        {
            return Value.ToString();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildBooleanNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    [NodeName("Boolean")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("BooleanDescription", typeof(Resources))]
    [NodeSearchTags("BooleanSelectorSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.BoolSelector", "DSCoreNodesUI.Input.BoolSelector")]
    public class BoolSelector : Bool
    {
        public BoolSelector()
        {
            Value = false;
            ShouldDisplayPreviewCore = false;
        }
    }
}
