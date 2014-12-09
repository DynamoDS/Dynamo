using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public abstract class Bool : BasicInteractive<bool>
    {
        protected Bool(WorkspaceModel workspace) : base(workspace) { }

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

        protected override string SerializeValue()
        {
            return this.Value.ToString();
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
    [NodeDescription("Selection between a true and false.")]
    [NodeSearchTags("true", "truth", "false")]
    [IsDesignScriptCompatible]
    public class BoolSelector : Bool
    {
        public BoolSelector(WorkspaceModel workspace) : base(workspace)
        {
            Value = false;
        }

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }

    }
}
