using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Create List")]
    [NodeDescription("Makes a new list out of the given inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [IsDesignScriptCompatible]
    public class CreateList : VariableInputNode
    {
        public CreateList()
        {
            InPortData.Add(new PortData("index0", "Item Index #0"));
            OutPortData.Add(new PortData("list", "A list"));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputName(int index)
        {
            return "index" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "Item Index #" + index;
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildExprList(inputAstNodes))
            };
        }
    }
}
