using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Drawing;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using CoreNodeModels;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using DSCore.IO;
using System.Linq;

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

    [NodeName("Test Selection Node2")]
    [NodeCategory("TestUINodes")]
    [NodeDescription("A test selection node.")]
    [OutPortTypes("var")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class TestSelectionNode2 : SelectionBase<int, int>
    {
        public TestSelectionNode2() : base(
                SelectionType.One,
                SelectionObjectType.None,
                "message",
                "prefix")
        {
        }

        public override IModelSelectionHelper<int> SelectionHelper => throw new NotImplementedException();

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            Func<IList, int> func = DSCore.List.Count;

            var results = SelectionResults.ToList();

            if (SelectionResults == null || !results.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                node = AstFactory.BuildFunctionCall(func,
                    new List<AssociativeNode> { AstFactory.BuildExprList(SelectionResults.Select(i => AstFactory.BuildIntNode((long)i) as AssociativeNode).ToList()) });
            }

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node)
            };
        }

        protected override IEnumerable<int> ExtractSelectionResults(int selections)
        {
            return new List<int> { selections };
        }

        protected override string GetIdentifierFromModelObject(int modelObject)
        {
            return modelObject.ToString();
        }

        protected override int GetModelObjectFromIdentifer(string id)
        {
            return id.Length;
        }
    }

    [NodeName("Test Dropdown Node")]
    [NodeCategory("TestUINodes")]
    [NodeDescription("test dropdown node")]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class TestDropdown : DSDropDownBase
    {
        public TestDropdown() : base("TestDropdown") { }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                node = AstFactory.BuildNullNode();
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
            }
            else
            {
                // get the selected items name
                var stringNode = AstFactory.BuildStringNode((string)Items[SelectedIndex].Name);

                // assign the selected name to an actual enumeration value
                var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode);

                // return the enumeration value
                return new List<AssociativeNode> { assign };
            }
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var symbols = new[] { "one", "two", "three" };
            

            foreach (var symbol in symbols)
            {

                Items.Add(new DynamoDropDownItem(symbol, symbol));
            }

            return SelectionState.Restore;
        }

    }
}
