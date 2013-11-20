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
    public class ASTNodesToCode : IAstNodeContainer
    {
        public List<AssociativeNode> AstNodes { get; set; }

        public ASTNodesToCode()
        {
            AstNodes = new List<AssociativeNode>();
        }

        public void OnAstNodeBuilding(Guid node)
        {

        }

        public void OnAstNodeBuilt(Guid node, IEnumerable<AssociativeNode> astNodes)
        {
            AstNodes.AddRange(astNodes);
        }
    }

    public static class NodeToCodeUtils
    {
        public static string ConvertNodesToCode(IEnumerable<NodeModel> nodeList)
        {
            var astnodesContainer = new ASTNodesToCode();
            var astBuilder = new AstBuilder(astnodesContainer);
            astBuilder.CompileToAstNodes(nodeList, false);

            var astNodes = astnodesContainer.AstNodes;

            string code = GraphToDSCompiler.GraphUtilities.ASTListToCode(astNodes);
            return code;
        }
    }
}
