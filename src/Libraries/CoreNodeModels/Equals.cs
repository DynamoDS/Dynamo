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
        private static readonly DoubleNode tolerancePortDefaultValue = new DoubleNode(MathUtils.Tolerance);

        [JsonConstructor]
        private Equals(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Auto;
            inPorts.ElementAt(2).DefaultValue = tolerancePortDefaultValue;
        }

        public Equals()
        {
            ArgumentLacing = LacingStrategy.Auto;

            InPorts.Add(new PortModel(PortType.Input, this, new PortData("lhs", "integer or double value")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("rhs", "integer or double value")));
            InPorts.Add(new PortModel(PortType.Input, this,
                new PortData("tolerance", "tolerance", tolerancePortDefaultValue)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("bool", "result of equality check")));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected && !InPorts[2].IsConnected)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            AssociativeNode rhs;
            if (IsPartiallyApplied)
            {
                var connectedInputs = Enumerable.Range(0, InPorts.Count)
                    .Where(index => InPorts[index].IsConnected)
                    .Select(x => new IntNode(x) as AssociativeNode)
                    .ToList();
                var arguments = AstFactory.BuildExprList(inputAstNodes);
                

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
            else
            {
                UseLevelAndReplicationGuide(inputAstNodes);

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
