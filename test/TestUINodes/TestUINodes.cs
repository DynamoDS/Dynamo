using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestUINodes 
{
    [NodeName("NodeWithFailingASTOutput")]
    [NodeCategory("TestUINodes")]
    [NodeDescription("A test UI node which will throw an excpetion when it is compiled to AST node.")]
    [IsVisibleInDynamoLibrary(false)]
    public class NodeWithFailingASTOutput: NodeModel
    {
        public NodeWithFailingASTOutput()
        {
            InPortData.Add(new PortData("input", "dummy input"));
            OutPortData.Add(new PortData("result", "dummy result"));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new Exception("Dummy error message.");
        }
    }
}
