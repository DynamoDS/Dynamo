using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.DSEngine
{
    public class Nodes2CodeUtils
    {
        public static IEnumerable<AssociativeNode> Node2Code(AstBuilder astBuilder, IEnumerable<NodeModel> nodes, bool verboseLogging)
        {
            return astBuilder.CompileToAstNodes(nodes, false, verboseLogging, false);
        }
    }
}
