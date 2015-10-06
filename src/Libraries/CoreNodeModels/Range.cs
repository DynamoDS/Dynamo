using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Properties;
using ProtoCore.AST.AssociativeAST;

using System.Collections.Generic;

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
                        FromNode = inputAstNodes[0],
                        ToNode = inputAstNodes[1],
                        StepNode = inputAstNodes[2],
                        stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize
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
                        FromNode = inputAstNodes[0],
                        ToNode = inputAstNodes[1],
                        StepNode = inputAstNodes[2],
                        HasRangeAmountOperator = true,
                        stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize                     
                    })
            };
        }
    }
}
