using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using CodeBlockNode = ProtoCore.AST.AssociativeAST.CodeBlockNode;
using LanguageBlockNode = ProtoCore.AST.AssociativeAST.LanguageBlockNode;

namespace DSCoreNodesUI.Logic
{
    [NodeName("If"), NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL),
     NodeDescription("Conditional statement"), IsDesignScriptCompatible]
    public class If : ScopedNodeModel
    {
        public If()
        {
            InPortData.Add(new PortData("test", "Test block"));
            InPortData.Add(new PortData("true", "True block"));
            InPortData.Add(new PortData("false", "False block"));

            OutPortData.Add(new PortData("result", "Result"));

            RegisterAllPorts();

            //TODO: Default Values
        }

        private List<ProtoCore.AST.ImperativeAST.ImperativeNode> GetAstsForBranch(
                                                                    int branch, 
                                                                    List<AssociativeNode> inputAstNodes)
        {
            AstBuilder astBuilder = new AstBuilder(null);

            // Get all upstream nodes and then remove nodes that are not 
            var nodes = GetInScopeNodesForInport(branch).Where(n => !(n is Symbol));
            nodes = ScopedNodeModel.RemoveInScopedNodeFrom(nodes);
            var astNodes = astBuilder.CompileToAstNodes(nodes, false, false);
            astNodes.Add(AstFactory.BuildReturnStatement(inputAstNodes[branch]));
            return astNodes.Select(n => n.ToImperativeAST()).ToList();
        }

        /// <summary>
        /// Specify if upstream nodes that connected to specified inport should
        /// be compiled in the scope or not. 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        protected override bool IsScopedInport(int portIndex)
        {
            return portIndex == 0 || portIndex == 1 || portIndex == 2;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAstInScope(List<AssociativeNode> inputAstNodes)
        {
            // This function will compile IF node to the following format:
            //
            //     v = [Associative]
            //     {
            //         return = [Imperative]
            //         {
            //             conditions...
            //             if (cond) { 
            //                 ...
            //             }
            //             else {
            //                 ...
            //             }
            //         }
            //     }
            //
            // The reason for outer associative language block is, if there are
            // some nested if nodes, as the language doesn't allow to nested
            // imperative block directly inside an imperative block, we have to
            // pass the language context to all children nodes, and children
            // may generate different structure even they are the same node.
            // 
            // As the langauge allows to nest associative block in global 
            // (associative) block, adding this extra associative language 
            // coudl avoid some troubles in context analysis.
            var conditions = GetAstsForBranch(0, inputAstNodes);
            // remove last return statement
            conditions.RemoveAt(conditions.Count - 1);

            var astsInTrueBranch = GetAstsForBranch(1, inputAstNodes);
            var astsInFalseBranch = GetAstsForBranch(2, inputAstNodes);

            var body = conditions;
            var ifelseStatement = new ProtoCore.AST.ImperativeAST.IfStmtNode()
            {
                IfExprNode = inputAstNodes[0].ToImperativeAST(),
                IfBody = astsInTrueBranch,
                ElseBody = astsInFalseBranch
            };
            body.Add(ifelseStatement);

            var innerImperativeBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.kImperative),
                CodeBlockNode = new ProtoCore.AST.ImperativeAST.CodeBlockNode
                {
                    Body = body
                }
            };

            var outerAssociativeBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.kAssociative),
                CodeBlockNode = new CodeBlockNode
                {
                    Body = new List<AssociativeNode>
                    {
                        AstFactory.BuildReturnStatement(innerImperativeBlock)
                    }
                }
            };
            var thisVariable = GetAstIdentifierForOutputIndex(0);
            var assignment = AstFactory.BuildAssignment(thisVariable, outerAssociativeBlock);

            return new AssociativeNode[] 
            {
                assignment
            };
        }  

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var lhs = GetAstIdentifierForOutputIndex(0);
            AssociativeNode rhs;

            if (HasUnconnectedInput())
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

                rhs = AstFactory.BuildFunctionCall("_SingleFunctionObject", inputParams);
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
