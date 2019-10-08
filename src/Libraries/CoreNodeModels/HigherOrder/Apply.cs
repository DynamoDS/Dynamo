using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.HigherOrder
{
    [NodeName("Function Apply")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("FunctionApplyDescription", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.HigherOrder.ApplyFunction", "Function.Apply")]
    public class ApplyFunction : VariableInputNode
    {
        [JsonConstructor]
        private ApplyFunction(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public ApplyFunction() : base()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("func", Resources.ApplyPortDataFuncToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("func(args)", Resources.ApplyPortDataFuncArgToolTip)));
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
            if (InPorts.Count > 1)
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

    [NodeName("Function Compose")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("FunctionComposeDescription", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.HigherOrder.ComposeFunctions", "Function.Compose")]
    public class ComposeFunctions : VariableInputNode
    {
        [JsonConstructor]
        private ComposeFunctions(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public ComposeFunctions()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("func0", Resources.ComposePortDataFunc0ToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("func1", Resources.ComposePortDataFunc1ToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("func", Resources.ComposePortDataResultToolTip)));
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
            if (InPorts.Count > 2)
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
