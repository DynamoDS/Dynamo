using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;
using Dynamo.Nodes;
using DSCoreNodesUI.Properties;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI.HigherOrder
{
    [NodeName("Function.Apply")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("FunctionApplyDescription", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    public class ApplyFunction : VariableInputNode
    {
        public ApplyFunction() : base()
        {
            InPortData.Add(new PortData("func", Resources.ApplyPortDataFuncToolTip));
            OutPortData.Add(new PortData("func(args)", Resources.ApplyPortDataFuncArgToolTip));
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
    [NodeDescription("FunctionComposeDescription", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    public class ComposeFunctions : VariableInputNode
    {
        public ComposeFunctions()
        {
            InPortData.Add(new PortData("func0", Resources.ComposePortDataFunc0ToolTip));
            InPortData.Add(new PortData("func1", Resources.ComposePortDataFunc1ToolTip));

            OutPortData.Add(new PortData("func", Resources.ComposePortDataResultToolTip));
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
