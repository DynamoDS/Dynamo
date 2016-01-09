using System.Linq;
using Dynamo.Properties;
using ProtoCore.AST.AssociativeAST;

using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using ProtoCore.DSASM;

namespace CoreNodeModels
{
    [NodeName("Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("RangeDescription", typeof(Resources))]
    [NodeSearchTags("RangeSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Range")]
    public class Range : NodeModel
    {
        private readonly IntNode startPortDefaultValue = new IntNode(0);
        private readonly IntNode endPortDefaultValue = new IntNode(9);
        private readonly IntNode stepPortDefaultValue = new IntNode(1);

        public Range()
        {
            InPortData.Add(new PortData("start", Resources.RangePortDataStartToolTip, startPortDefaultValue));
            InPortData.Add(new PortData("end", Resources.RangePortDataEndToolTip, endPortDefaultValue));
            InPortData.Add(new PortData("step", Resources.RangePortDataStepToolTip, stepPortDefaultValue));
            OutPortData.Add(new PortData("seq", Resources.RangePortDataSeqToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                var connectedPorts = Enumerable.Range(0, this.InPorts.Count)
                    .Where(this.HasInput)
                    .ToList();

                // 3d, 4th, 5th are always connected.
                connectedPorts.AddRange(new List<int> { 3, 4, 5 });
                return new[]
                {
                     AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                             AstFactory.BuildFunctionObject(
                                Constants.kFunctionRangeExpression,
                                6,
                                connectedPorts,
                                new List<AssociativeNode>
                                {
                                    inputAstNodes[0],
                                    inputAstNodes[1],
                                    inputAstNodes[2],
                                    new IntNode(0),
                                    new BooleanNode(true),
                                    new BooleanNode(false)
                                }))
                };
            }
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new RangeExprNode
                    {
                        From = inputAstNodes[0],
                        To = inputAstNodes[1],
                        Step = inputAstNodes[2],
                        StepOperator = ProtoCore.DSASM.RangeStepOperator.StepSize
                    })
            };
        }
    }

    [NodeName("Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("SequenceDescription", typeof(Resources))]
    [NodeSearchTags("SequenceSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Sequence")]
    public class Sequence : NodeModel
    {
        private readonly IntNode startPortDefaultValue = new IntNode(0);
        private readonly IntNode amountPortDefaultValue = new IntNode(10);
        private readonly IntNode stepPortDefaultValue = new IntNode(1);

        public Sequence()
        {
            InPortData.Add(new PortData("start", Resources.RangePortDataStartToolTip, startPortDefaultValue));
            InPortData.Add(new PortData("amount", Resources.RangePortDataAmountToolTip, amountPortDefaultValue));
            InPortData.Add(new PortData("step", Resources.RangePortDataStepToolTip, stepPortDefaultValue));
            OutPortData.Add(new PortData("seq", Resources.RangePortDataSeqToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                var connectedPorts = Enumerable.Range(0, this.InPorts.Count)
                    .Where(this.HasInput)
                    .ToList();

                // 3d, 4th, 5th are always connected.
                connectedPorts.AddRange(new List<int> { 3, 4, 5 });
                return new[]
                {
                     AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                             AstFactory.BuildFunctionObject(
                                Constants.kFunctionRangeExpression,
                                6,
                                connectedPorts,
                                new List<AssociativeNode>
                                {
                                    inputAstNodes[0],
                                    inputAstNodes[1],
                                    inputAstNodes[2],
                                    new IntNode(0),
                                    new BooleanNode(true),
                                    new BooleanNode(true)
                                }))
                };
            }
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new RangeExprNode 
                    {
                        From = inputAstNodes[0],
                        To = inputAstNodes[1],
                        Step = inputAstNodes[2],
                        HasRangeAmountOperator = true,
                        StepOperator = ProtoCore.DSASM.RangeStepOperator.StepSize                     
                    })
            };
        }
    }
}
