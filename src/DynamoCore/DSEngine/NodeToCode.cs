using System.Collections.Generic;

using Dynamo.Models;

namespace Dynamo.DSEngine
{
    public static class NodeToCodeUtils
    {
        public static string ConvertNodesToCode(AstBuilder astBuilder, IEnumerable<NodeModel> nodeList, bool verboseLogging)
        {
            var astNodes = astBuilder.CompileToAstNodes(nodeList, false, verboseLogging);
            var codeGen = new ProtoCore.CodeGenDS(astNodes);
            return codeGen.GenerateCode();
        }
    }
}
