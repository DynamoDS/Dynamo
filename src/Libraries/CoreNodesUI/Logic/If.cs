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

        private FunctionDefinitionNode CreateFunctionDef(IEnumerable<string> parameters,
                                                         List<AssociativeNode> innerFunctionDefs,
                                                         int branch)
        {
            AstBuilder astBuilder = new AstBuilder(null);
            var nodesInBranch = GetInScopeNodesForInport(branch);
            var astNodes = astBuilder.CompileToAstNodes(nodesInBranch, false, false);

            var astNodesInBranch = new List<AssociativeNode>();
            SplitFunctionNodesFromAsts(astNodes, astNodesInBranch, innerFunctionDefs);

            var functionBody = new CodeBlockNode();
            functionBody.Body.AddRange(astNodesInBranch);

            var varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar);

            var funcionDef = new FunctionDefinitionNode
            {
                Name = "IF_" + this.GUID.ToString().Replace("-", string.Empty) + "_" + branch.ToString(),
                Signature = new ArgumentSignatureNode
                {
                    Arguments = parameters.Select(param => AstFactory.BuildParamNode(param, varType)).ToList()
                },
                FunctionBody = functionBody,
                ReturnType = varType 
            };

            return funcionDef;
        }

        private void SplitFunctionNodesFromAsts(List<AssociativeNode> allAstNodes,
                                                List<AssociativeNode> astNodes,
                                                List<AssociativeNode> functionNodes)
        {
            foreach (var astNode in allAstNodes)
            {
                if (astNode is FunctionDefinitionNode)
                {
                    functionNodes.Add(astNode);
                }
                else
                {
                    astNodes.Add(astNode);
                }
            }
        }

        protected override bool IsScopedInPort(int portIndex)
        {
            return portIndex == 1 || portIndex == 2;
        }

        private IEnumerable<AssociativeNode> BuildNodesInScope(List<AssociativeNode> inputAstNodes)
        {
            var workspace = dynSettings.Controller.DynamoModel.CurrentWorkspace;
            var inputNodes = workspace.Nodes.OfType<Symbol>().ToList();
            var paramNames = inputNodes.Select(x => string.IsNullOrEmpty(x.InputSymbol) ? x.AstIdentifierBase : x.InputSymbol);

            List<AssociativeNode> innerFuncs = new List<AssociativeNode>();

            //Create a new function definition for true branch
            var trueFunc = CreateFunctionDef(paramNames, innerFuncs, 1);
            var trueRet = AstFactory.BuildReturnStatement(inputAstNodes[1]);
            trueFunc.FunctionBody.Body.Add(trueRet);

            //Create a new function definition for true branch
            var falseFunc = CreateFunctionDef(paramNames, innerFuncs, 2);
            var falseRet = AstFactory.BuildReturnStatement(inputAstNodes[2]);
            falseFunc.FunctionBody.Body.Add(falseRet);

            var parameters = inputNodes.Select(x => AstFactory.BuildIdentifier(x.AstIdentifierBase))
                                       .Cast<AssociativeNode>()
                                       .ToList(); 

            var lhs = GetAstIdentifierForOutputIndex(0);
            var rhs = new InlineConditionalNode
            {
                ConditionExpression = inputAstNodes[0],
                TrueExpression = AstFactory.BuildFunctionCall(trueFunc.Name, parameters),
                FalseExpression = AstFactory.BuildFunctionCall(falseFunc.Name, parameters),
            };
            var assignment = AstFactory.BuildAssignment(lhs, rhs);

            List<AssociativeNode> finalAsts = new List<AssociativeNode>();
            finalAsts.AddRange(innerFuncs);
            finalAsts.Add(trueFunc);
            finalAsts.Add(falseFunc);
            finalAsts.Add(assignment);

            return finalAsts;
        }  

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (dynSettings.Controller.DynamoModel.CurrentWorkspace is CustomNodeWorkspaceModel)
                return BuildNodesInScope(inputAstNodes);

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
