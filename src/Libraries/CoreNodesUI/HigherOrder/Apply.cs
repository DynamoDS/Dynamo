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
                        Function = AstFactory.BuildIdentifier("ApplyList"),
                        FormalArguments =
                            new List<AssociativeNode>
                            {
                                inputAstNodes[0],
                                AstFactory.BuildExprList(inputAstNodes.Skip(1).ToList())
                            }
                    })
            };
        }
    }

    [NodeName("Compose Function")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Compose multiple functions.")]
    [IsDesignScriptCompatible]
    public class ComposeFunctions : VariableInputNode
    {
        public ComposeFunctions()
        {
            InPortData.Add(new PortData("func0", "Function #0"));
            InPortData.Add(new PortData("func1", "Function #1"));

            OutPortData.Add(new PortData("func", "Composed function."));
            RegisterAllPorts();
        }

        protected override string GetInputName(int index)
        {
            return "func" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "Function #" + index;
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__Compose",
                        new List<AssociativeNode>
                        {
                            AstFactory.BuildExprList(inputAstNodes)
                        }))
            };
        }
    }
}
