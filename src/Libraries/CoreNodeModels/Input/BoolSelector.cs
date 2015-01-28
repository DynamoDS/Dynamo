﻿using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Nodes;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
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
    [NodeDescription("BooleanDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("BooleanSelectorSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class BoolSelector : Bool
    {
        public BoolSelector()
        {
            Value = false;
            ShouldDisplayPreviewCore = false;
        }
    }
}
