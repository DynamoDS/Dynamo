using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI.HigherOrder
{
    [NodeName("Function.Apply")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to arguments.")]
    [IsDesignScriptCompatible]
    public class ApplyFunction : VariableInputNode
    {
        public ApplyFunction() : base()
        {
            InPortData.Add(new PortData("func", "Function to apply."));
            OutPortData.Add(new PortData("func(args)", "Result of application."));
            AddInput();
            RegisterAllPorts();
        }

        protected override string GetInputName(int index)
        {
            if (index == 0)
                return "func";

            return "arg" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            if (index == 0)
                return "Function to apply.";

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
                        Function = AstFactory.BuildIdentifier("__ApplyList"),
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

    [NodeName("Function.Compose")]
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
                            AstFactory.BuildExprList(Enumerable.Reverse(inputAstNodes).ToList())
                        }))
            };
        }
    }
}
