using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoDummyNodes
{
    [NodeName("DynamoDummyErrorNode")]
    [NodeCategory("DynamoDummy")]
    [NodeDescription("Dynamo dummy node which will throuw an excpetion when is compiled to AST.")]
    [IsDesignScriptCompatible]
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
            throw new Exception("Dummy error in building dummy node");
        }
    }
}
