using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

    }
}
