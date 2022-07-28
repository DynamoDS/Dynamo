using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Drawing;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using DSCore.IO;

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
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("input", "dummy input")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("result", "dummy result")));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new Exception("Dummy error message.");
        }
    }

    [NodeName("Test Selection Node")]
    [NodeCategory("TestUINodes")]
    [NodeDescription("A test selection node.")]
    [OutPortTypes("var")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class TestSelectionNode : NodeModel
    {
        public TestSelectionNode()
        {
            // Nodes can have an arbitrary number of inputs and outputs.
            // If you want more ports, just create more PortData objects.
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("File", "The selected file.")));

            // This call is required to ensure that your ports are
            // properly created.
            RegisterAllPorts();

            // The arugment lacing is the way in which Dynamo handles
            // inputs of lists. If you don't want your node to
            // support argument lacing, you can set this to LacingStrategy.Disabled.
            ArgumentLacing = LacingStrategy.Disabled;

            IsSetAsInput = true;
        }

        // Starting with Dynamo v2.0 you must add Json constructors for all nodeModel
        // dervived nodes to support the move from an Xml to Json file format.  Failing to
        // do so will result in incorrect ports being generated upon serialization/deserialization.
        // This constructor is called when opening a Json graph.
        [JsonConstructor]
        TestSelectionNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(executingDirectory);
            var testDirectory = Path.Combine(directory.Parent.Parent.Parent.Parent.FullName, "test/core/astbuilder");

            string imagePath = Path.Combine(testDirectory, "hardcoded_image_file.jpg");
            var func1 =
                AstFactory.BuildFunctionCall(
                    new Func<string, FileInfo>(FileSystem.FileFromPath),
                    new List<AssociativeNode> { AstFactory.BuildStringNode(imagePath) });

            var func2 = AstFactory.BuildFunctionCall(
                new Func<FileInfo, Bitmap>(DSCore.IO.Image.ReadFromFile), new List<AssociativeNode> { func1 });

            // returns an identifier that is assigned to a bitmap
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), func2),

            };
        }
    }
}
