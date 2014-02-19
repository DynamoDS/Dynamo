using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI.HigherOrder
{
    [NodeName("Apply Function")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to arguments.")]
    [IsDesignScriptCompatible]
    public class ApplyFunction : VariableInputNode
    {
        public ApplyFunction()
        {
            InPortData.Add(new PortData("func", "Function to apply."));
            OutPortData.Add(new PortData("func(args)", "Result of application."));
            RegisterAllPorts();
        }

        protected override string GetInputName(int index)
        {
            return "arg" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "Argument #" + index;
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
                    new FunctionCallNode
                    {
                        Function = inputAstNodes[0],
                        FormalArguments = inputAstNodes.Skip(1).ToList()
                    })
            };
        }
    }
}
