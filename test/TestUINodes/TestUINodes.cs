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
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("File", "The selected file.")));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            IsSetAsInput = true;
        }

        [JsonConstructor]
        TestSelectionNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

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

            // returns an identifier that is assigned to a bitmap upon the call to Image.ReadFromFile.
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), func2),

            };
        }
    }
}
