using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoDummyNodes
{
    [NodeName("DynamoDummyErrorNode")]
    [NodeCategory("DynamoDummyNodes")]
    [NodeDescription("A test node which will throw an excpetion when it is compiled to AST node.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class DynamoDummyErrorNode: NodeModel
    {
        public DynamoDummyErrorNode(WorkspaceModel workspaceModel)
            : base(workspaceModel)
        {
            InPortData.Add(new PortData("dummyInput", "dummy"));
            OutPortData.Add(new PortData("dummyOutput", "Result"));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new Exception("Dummy error message.");
        }
    }
}
