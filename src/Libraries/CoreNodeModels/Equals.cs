using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace CoreNodeModels
{
    [NodeName("==")]
    [NodeDescription("Equals with tolerance input")]
    [NodeCategory("Core.Math")]
    [NodeSearchTags("Equals")]
    [InPortTypes("double ", "double", "double")]
    [OutPortTypes("bool")]
    [IsDesignScriptCompatible]
    public class Equals : NodeModel
    {
        private static readonly string functionName = nameof(DSCore.List.Equals);

        [JsonConstructor]
        private Equals(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public Equals()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("lhs", "integer or double value")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("rhs", "integer or double value")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("tolerance", "tolerance = 0.00001")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("bool", "result of equality check")));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected && !InPorts[2].IsConnected)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            AssociativeNode rhs = null;
            if (IsPartiallyApplied)
            {
                if (inputAstNodes[0].Kind != AstKind.Null && inputAstNodes[1].Kind != AstKind.Null && inputAstNodes[2].Kind == AstKind.Null)
                {
                    rhs = AstFactory.BuildFunctionCall(new Func<double, double, double, bool>(DSCore.Math.Equals),
                        new List<AssociativeNode> {inputAstNodes[0], inputAstNodes[1]});
                }
                else
                {
                    List<AssociativeNode> connectedInputs = null;
                    ExprListNode arguments = null;
                    if (inputAstNodes[2].Kind == AstKind.Null)
                    {
                        connectedInputs = Enumerable.Range(0, InPorts.Count - 1)
                            .Where(index => InPorts[index].IsConnected)
                            .Select(x => new IntNode(x) as AssociativeNode)
                            .ToList();
                        connectedInputs.Add(new IntNode(2));
                        arguments = AstFactory.BuildExprList(new List<AssociativeNode>
                            {inputAstNodes[0], inputAstNodes[1], new DoubleNode(MathUtils.Tolerance)});
                    }
                    else
                    {
                        connectedInputs = Enumerable.Range(0, InPorts.Count)
                            .Where(index => InPorts[index].IsConnected)
                            .Select(x => new IntNode(x) as AssociativeNode)
                            .ToList();
                        arguments = AstFactory.BuildExprList(inputAstNodes);
                    }

                    var functionNode = new IdentifierListNode
                    {
                        LeftNode = new IdentifierNode("DSCore.Math"),
                        RightNode = new IdentifierNode("Equals")
                    };
                    IntNode paramNumNode = new IntNode(3);
                    var positionNode = AstFactory.BuildExprList(connectedInputs);
                    
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
            }
            else
            {
                rhs = AstFactory.BuildFunctionCall(new Func<double, double, double, bool>(DSCore.Math.Equals),
                    inputAstNodes);
            }

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs)
            };
        }
    }
}
