using System.Collections.Generic;
using System.Linq;
using CoreNodeModels.Properties;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Newtonsoft.Json;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using CodeBlockNode = ProtoCore.AST.AssociativeAST.CodeBlockNode;
using LanguageBlockNode = ProtoCore.AST.AssociativeAST.LanguageBlockNode;

namespace CoreNodeModels.Logic
{
    [NodeName("ScopeIf"), NodeCategory(BuiltinNodeCategories.LOGIC),
    NodeDescription("ScopeIfDescription", typeof(Resources)), IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Logic.ScopedIf")]
    [OutPortTypes("Function")]
    public class ScopedIf : ScopedNodeModel
    {
        [JsonConstructor]
        private ScopedIf(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public ScopedIf() : base()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("test", Resources.PortDataTestBlockToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("true", Resources.PortDataTrueBlockToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("false", Resources.PortDataFalseBlockToolTip)));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("result", Resources.PortDataResultToolTip)));
            RegisterAllPorts();
        }

        private List<AssociativeNode> GetAstsForBranch(int branch, List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {
            // Get all upstream nodes and then remove nodes that are not 
            var nodes = GetInScopeNodesForInport(branch, false).Where(n => !(n is Symbol));
            nodes = ScopedNodeModel.GetNodesInTopScope(nodes);

            // The second parameter, isDeltaExecution, is set to false so that
            // all AST nodes will be added to this IF graph node instead of 
            // adding to the corresponding graph node. 
            var allAstNodes = builder.CompileToAstNodes(nodes, CompilationContext.None, verboseLogging);
            var astNodes = allAstNodes.SelectMany(t => t.Item2).ToList();
            astNodes.Add(AstFactory.BuildReturnStatement(inputAstNodes[branch]));
            return astNodes;
        }

        private void SanityCheck()
        {
            // condition branch
            var condNodes = GetInScopeNodesForInport(0, false, true, true).Where(n => !(n is Symbol));
            var trueNodes = new HashSet<NodeModel>(GetInScopeNodesForInport(1, false).Where(n => !(n is Symbol)));
            var falseNodes = new HashSet<NodeModel>(GetInScopeNodesForInport(2, false).Where(n => !(n is Symbol)));

            trueNodes.IntersectWith(condNodes);
            falseNodes.IntersectWith(condNodes);
            trueNodes.UnionWith(falseNodes);

            if (trueNodes.Any())
            {
                foreach (var node in trueNodes)
                {
                    node.Error("A node cann't be both in condition and true/false branches of IF node");
                }
            }
        }

        /// <summary>
        /// Specify if upstream nodes that connected to specified inport should
        /// be compiled in the scope or not. 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        protected override bool IsScopedInport(int portIndex)
        {
            return portIndex == 1 || portIndex == 2;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAstInScope(List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {
            // This function will compile IF node to the following format:
            //
            //     cond = ...;
            //     v = [Imperative]
            //     {
            //         if (cond) {
            //             return = [Associative] {
            //                 ...
            //             }
            //         }
            //         else {
            //             return = [Associative] {
            //                 ...
            //             }
            //         }
            //     }
            //

            var astsInTrueBranch = GetAstsForBranch(1, inputAstNodes, verboseLogging, builder);
            var astsInFalseBranch = GetAstsForBranch(2, inputAstNodes, verboseLogging, builder);

            // if (cond) {
            //     return = [Associative] {...}
            // }
            var ifBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Associative),
                CodeBlockNode = new CodeBlockNode { Body = astsInTrueBranch }
            };
            var ifBranch = AstFactory.BuildReturnStatement(ifBlock).ToImperativeAST();

            // else {
            //     return = [Associative] { ... }
            // }
            var elseBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Associative),
                CodeBlockNode = new CodeBlockNode { Body = astsInFalseBranch }
            };
            var elseBranch = AstFactory.BuildReturnStatement(elseBlock).ToImperativeAST();

            var ifelseStatement = new ProtoCore.AST.ImperativeAST.IfStmtNode()
            {
                IfExprNode = inputAstNodes[0].ToImperativeAST(),
                IfBody = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> { ifBranch },
                ElseBody = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> { elseBranch }
            };

            // thisVariable = [Imperative]
            // {
            //     ...
            // }
            var outerBlock = new LanguageBlockNode
            {
                codeblock = new LanguageCodeBlock(Language.Imperative),
                CodeBlockNode = new ProtoCore.AST.ImperativeAST.CodeBlockNode
                {
                    Body = new List<ProtoCore.AST.ImperativeAST.ImperativeNode> { ifelseStatement }
                }
            };

            var thisVariable = GetAstIdentifierForOutputIndex(0);
            var assignment = AstFactory.BuildAssignment(thisVariable, outerBlock);

            return new AssociativeNode[] 
            {
                assignment
            };
        }
    }
}