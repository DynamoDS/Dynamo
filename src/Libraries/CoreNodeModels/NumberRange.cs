using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Properties;
using ProtoCore.AST.AssociativeAST;

using System.Collections.Generic;

namespace DSCoreNodesUI
{
    [NodeName("Number Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("NumberRangeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class NumberRange : NodeModel
    {
        public NumberRange()
        {
            InPortData.Add(new PortData("start", Resources.NumberRangePortDataStartToolTip));
            InPortData.Add(new PortData("end", Resources.NumberRangePortDataEndToolTip));
            InPortData.Add(new PortData("step", Resources.NumberRangePortDataStepToolTip));
            OutPortData.Add(new PortData("seq", Resources.NumberRangePortDataSeqToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
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

    [NodeName("Number Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("NumberSequenceDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class NumberSeq : NodeModel
    {
        public NumberSeq()
        {
            InPortData.Add(new PortData("start", Resources.NumberRangePortDataStartToolTip));
            InPortData.Add(new PortData("amount", Resources.NumberRangePortDataAmountToolTip));
            InPortData.Add(new PortData("step", Resources.NumberRangePortDataStepToolTip));
            OutPortData.Add(new PortData("seq", Resources.NumberRangePortDataSeqToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
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
