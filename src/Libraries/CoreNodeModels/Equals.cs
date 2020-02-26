using System;
using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace CoreNodeModels
{
    [NodeName("==")]
    [NodeDescription("EqualsWithToleranceDescription", typeof(Resources))]
    [NodeCategory("Core.Math")]
    [NodeSearchTags("EqualsWithToleranceSearchTags", typeof(Resources))]
    [InPortTypes("double", "double", "double")]
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

            InPorts.Add(new PortModel(PortType.Input, this, new PortData("x", Resources.EqualsWithToleranceLhsRhsTooltip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("y", Resources.EqualsWithToleranceLhsRhsTooltip)));
            InPorts.Add(new PortModel(PortType.Input, this,
                new PortData("tolerance", string.Format(Resources.EqualsWithToleranceTooltip, MathUtils.Tolerance), tolerancePortDefaultValue)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("bool", Resources.EqualsWithToleranceOutportTooltip)));
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
