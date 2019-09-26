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
                var connectedInputs = Enumerable.Range(0, InPorts.Count)
                                            .Where(index=>InPorts[index].IsConnected)
                                            .Select(x => new IntNode(x) as AssociativeNode)
                                            .ToList();
                var functionNode = new IdentifierNode(Constants.kInlineConditionalMethodName);
                var paramNumNode = new IntNode(3);
                var positionNode = AstFactory.BuildExprList(connectedInputs);
                var arguments = AstFactory.BuildExprList(inputAstNodes);
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
                rhs = new InlineConditionalNode
                {
                    ConditionExpression = inputAstNodes[0],
                    TrueExpression = inputAstNodes[1],
                    FalseExpression = inputAstNodes[2]
                };
            }

            return new[]
            {
                AstFactory.BuildAssignment(lhs, rhs)
            };
        }
    }
}
