using System.Collections.Generic;
using Dynamo.Models;
using GraphToDSCompiler;

namespace Dynamo.DSEngine
{
    public static class NodeToCodeUtils
    {
        public static string ConvertNodesToCode(IEnumerable<NodeModel> nodeList)
        {
            var astBuilder = new AstBuilder(null);
            var astNodes = astBuilder.CompileToAstNodes(nodeList, false);

            string code = GraphUtilities.ASTListToCode(astNodes);
            return code;
        }
    }
}
