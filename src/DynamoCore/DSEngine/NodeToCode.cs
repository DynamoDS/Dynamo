using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSDefinitions;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    public class ASTNodestoCode : IAstNodeContainer
    {
        public List<AssociativeNode> AstNodes { get; set; }

        public ASTNodestoCode()
        {
            AstNodes = new List<AssociativeNode>();
        }

        public void OnAstNodeBuilding(NodeModel node)
        {

        }

        public void OnAstNodeBuilt(NodeModel node, IEnumerable<AssociativeNode> astNodes)
        {
            AstNodes.AddRange(astNodes);
        }
    }

    public static class NodeToCodeUtils
    {
        public static string ConvertNodesToCode(IEnumerable<NodeModel> nodeList)
        {
            ASTNodestoCode astnodesContainer = new ASTNodestoCode();
            Dynamo.DSEngine.AstBuilder astBuilder = new Dynamo.DSEngine.AstBuilder(astnodesContainer);
            astBuilder.CompileToAstNodes(nodeList);

            var astNodes = astnodesContainer.AstNodes;

            string code = GraphToDSCompiler.GraphUtilities.ASTListToCode(astNodes);
            return code;
        }
    }
}
