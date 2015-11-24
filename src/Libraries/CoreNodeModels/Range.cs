using Dynamo.Properties;
using ProtoCore.AST.AssociativeAST;

using System.Collections.Generic;
using Dynamo.Graph.Nodes;

namespace DSCoreNodesUI
{
    [NodeName("Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("RangeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("RangeSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class Range : NodeModel
    {
        public Range()
        {
            InPortData.Add(new PortData("start", Resources.RangePortDataStartToolTip));
            InPortData.Add(new PortData("end", Resources.RangePortDataEndToolTip));
            InPortData.Add(new PortData("step", Resources.RangePortDataStepToolTip));
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
    [NodeDescription("SequenceDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("SequenceSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class Sequence : NodeModel
    {
        public Sequence()
        {
            InPortData.Add(new PortData("start", Resources.RangePortDataStartToolTip));
            InPortData.Add(new PortData("amount", Resources.RangePortDataAmountToolTip));
            InPortData.Add(new PortData("step", Resources.RangePortDataStepToolTip));
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
