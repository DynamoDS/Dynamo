using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCoreNodesUI
{
    [NodeName("Number Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("Creates a sequence of numbers in the specified range.")]
    [IsDesignScriptCompatible]
    public class NumberRange : NodeModel
    {
        public NumberRange(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at"));
            InPortData.Add(new PortData("end", "Number to end the sequence at"));
            InPortData.Add(new PortData("step", "Space between numbers"));
            OutPortData.Add(new PortData("seq", "New sequence"));

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
    [NodeDescription("Creates a sequence of numbers.")]
    [IsDesignScriptCompatible]
    public class NumberSeq : NodeModel
    {
        public NumberSeq(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at"));
            InPortData.Add(new PortData("amount", "Amount of numbers in the sequence"));
            InPortData.Add(new PortData("step", "Space between numbers"));
            OutPortData.Add(new PortData("seq", "New sequence"));

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
