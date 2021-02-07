using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace CoreNodeModels.Logic
{
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC)]
    [NodeDescription("IfDescription", typeof(Resources))]
    [OutPortTypes("Function")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Logic.If")]
    public class If : NodeModel
    {
        [JsonConstructor]
        private If(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public If()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("test", Resources.PortDataTestBlockToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("true", Resources.PortDataTrueBlockToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("false", Resources.PortDataFalseBlockToolTip)));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("result", Resources.PortDataResultToolTip)));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var lhs = GetAstIdentifierForOutputIndex(0);
            AssociativeNode rhs;

            if (IsPartiallyApplied)
            {
                // Build partial function for If node if it were a single function call:
                // Get.ValueAtIndex([t, f], cond ? 0 : 1);
                var connectedInputs = new List<AssociativeNode>();
                var inputs = new List<AssociativeNode>();

                if (InPorts[1].IsConnected && InPorts[2].IsConnected)
                {
                    connectedInputs.Add(new IntNode(0));
                    inputs.Add(AstFactory.BuildExprList(new List<AssociativeNode>
                        {inputAstNodes[1], inputAstNodes[2]}));
                }
                if (InPorts[0].IsConnected)
                {
                    connectedInputs.Add(new IntNode(1));
                    inputs.Add(AstFactory.BuildConditionalNode(inputAstNodes[0], new IntNode(0), new IntNode(1)));
                }

                var functionNode = new IdentifierListNode
                {
                    LeftNode = new IdentifierNode(ProtoCore.AST.Node.BuiltinGetValueAtIndexTypeName),
                    RightNode = new IdentifierNode(ProtoCore.AST.Node.BuiltinValueAtIndexMethodName)
                };
                var paramNumNode = new IntNode(2);
                var positionNode = AstFactory.BuildExprList(connectedInputs);

                var args = new List<AssociativeNode>();
                args.Add(AstFactory.BuildExprList(new List<AssociativeNode>
                    {inputAstNodes[1], inputAstNodes[2]}));
                args.Add(AstFactory.BuildConditionalNode(inputAstNodes[0], new IntNode(0), new IntNode(1)));
                
                var arguments = AstFactory.BuildExprList(args);
                var inputParams = new List<AssociativeNode>
                {
                    functionNode,
                    paramNumNode,
                    positionNode,
                    arguments,
                    AstFactory.BuildBooleanNode(true)
                };

                rhs = AstFactory.BuildFunctionCall("__CreateFunctionObject", inputParams);
            }
            else
            {
                var conditional = AstFactory.BuildConditionalNode(inputAstNodes[0], new IntNode(0), new IntNode(1));
                var trueStmt = inputAstNodes[1];
                var falseStmt = inputAstNodes[2];
                var list = AstFactory.BuildExprList(new List<AssociativeNode> {trueStmt, falseStmt});
                rhs = AstFactory.BuildIndexExpression(list, conditional);
            }

            return new[]
            {
                AstFactory.BuildAssignment(lhs, rhs)
            };
        }
    }
}
