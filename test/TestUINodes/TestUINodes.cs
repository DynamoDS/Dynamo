using Autodesk.DesignScript.Runtime;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace TestUINodes
{
    [NodeName("NodeWithFailingASTOutput")]
    [NodeCategory("TestUINodes")]
    [NodeDescription("A test UI node which will throw an excpetion when it is compiled to AST node.")]
    [IsVisibleInDynamoLibrary(false)]
    public class NodeWithFailingASTOutput: NodeModel
    {
        [JsonConstructor]
        private NodeWithFailingASTOutput(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

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
