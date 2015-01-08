using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Properties;
using ProtoCore.AST.AssociativeAST;

using System.Collections.Generic;

namespace DSCoreNodesUI
{
    [NodeName(/*NXLT*/"Number Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription(/*NXLT*/"NumberRangeDescription", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    public class NumberRange : NodeModel
    {
        public NumberRange()
        {
            InPortData.Add(new PortData(/*NXLT*/"start", Resources.NumberRangePortDataStartToolTip));
            InPortData.Add(new PortData(/*NXLT*/"end", Resources.NumberRangePortDataEndToolTip));
            InPortData.Add(new PortData(/*NXLT*/"step", Resources.NumberRangePortDataStepToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"seq", Resources.NumberRangePortDataSeqToolTip));

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

    [NodeName(/*NXLT*/"Number Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription(/*NXLT*/"NumberSequenceDescription", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    public class NumberSeq : NodeModel
    {
        public NumberSeq()
        {
            InPortData.Add(new PortData(/*NXLT*/"start", Resources.NumberRangePortDataStartToolTip));
            InPortData.Add(new PortData(/*NXLT*/"amount", Resources.NumberRangePortDataAmountToolTip));
            InPortData.Add(new PortData(/*NXLT*/"step", Resources.NumberRangePortDataStepToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"seq", Resources.NumberRangePortDataSeqToolTip));

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
