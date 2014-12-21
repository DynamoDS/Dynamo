using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using DSCoreNodesUI.Properties; 
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using CodeBlockNode = ProtoCore.AST.AssociativeAST.CodeBlockNode;
using LanguageBlockNode = ProtoCore.AST.AssociativeAST.LanguageBlockNode;

namespace DSCoreNodesUI.Logic
{
    [NodeName(/*NXLT*/"If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC)]
    [NodeDescription(/*NXLT*/"IfDescription", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    public class If : NodeModel
    {
        public If(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            InPortData.Add(new PortData(/*NXLT*/"test", Resources.PortDataTestBlockToolTip));
            InPortData.Add(new PortData(/*NXLT*/"true", Resources.PortDataTrueBlockToolTip));
            InPortData.Add(new PortData(/*NXLT*/"false", Resources.PortDataFalseBlockToolTip));

            OutPortData.Add(new PortData(/*NXLT*/"result", Resources.PortDataResultToolTip));

            RegisterAllPorts();

            //TODO: Default Values
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var lhs = GetAstIdentifierForOutputIndex(0);
            AssociativeNode rhs;

            if (IsPartiallyApplied)
            {
                var connectedInputs = Enumerable.Range(0, InPortData.Count)
                                            .Where(HasConnectedInput)
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

                rhs = AstFactory.BuildFunctionCall(/*NXLT*/"_SingleFunctionObject", inputParams);
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
