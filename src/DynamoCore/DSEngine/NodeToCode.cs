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
    public static class NodeToCodeUtils
    {
        public static string ConvertNodesToCode(DynamoModel dynamoModel, IEnumerable<NodeModel> nodeList)
        {
            var astBuilder = new AstBuilder(dynamoModel, null);
            var astNodes = astBuilder.CompileToAstNodes(nodeList, false);

            string code = GraphToDSCompiler.GraphUtilities.ASTListToCode(astNodes);
            return code;
        }
    }
}
