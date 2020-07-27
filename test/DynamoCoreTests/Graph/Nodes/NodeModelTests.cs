using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using Dynamo.Selection;
using NUnit.Framework;
using Revit.Elements;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the NodeModel class
    /// </summary>
    [TestFixture]
    class NodeModelTests : DynamoModelTestBase
    {
        NodeModel testNodeModel;

        [SetUp]
        public void Init()
        {
            CurrentDynamoModel.NodeFactory.CreateNodeFromTypeName("CoreNodeModels.Input.DoubleInput", out testNodeModel);
        }

        [Test]
        [Category("UnitTests")]
        public void CreationNameTest()
        {
            testNodeModel.Category = "Category";

            var result = testNodeModel.ConstructDictionaryLinkFromLibrary(CurrentDynamoModel.LibraryServices);
            Assert.AreEqual("http://dictionary.dynamobim.com/2/#/Category/Action/Number", result);
        }
        
        [Test]
        [Category("UnitTests")]
        public void CategoryTest()
        {
            testNodeModel.Category = null; //Set Category to null so that GetCategoryStringFromAttributes() gets called in the property getter
            var category = testNodeModel.Category;

            Assert.AreEqual("Core.Input", category);
        }

        [Test]
        [Category("UnitTests")]
        public void DictionaryLinkTest()
        {
            testNodeModel.DictionaryLink = null; //Set DictionaryLink to null to cover the setter and to test the Configuration property
            var dictLink = testNodeModel.DictionaryLink;

            Assert.AreEqual("http://dictionary.dynamobim.com/2/", dictLink);
        }

        [Test]
        [Category("UnitTests")]
        public void SelectNeighborsTest()
        {
            //Open and run the workspace
            string listTestFolder = Path.Combine(TestDirectory, "core");
            string testFilePath = Path.Combine(listTestFolder, "Angle.dyn");

            RunModel(testFilePath);

            //Select the node with neighbors
            NodeModel node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("dcd9c6c6-6350-4292-a553-c57a764504b4");

            var countBefore = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(0, countBefore);

            //Run the method and assert whether more nodes were selected
            node.SelectNeighbors();

            var countAfter = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(2, countAfter);
        }

        /// <summary>
        /// Class created in order to test protected methods in the NodeModel parent
        /// </summary>
        private class NodeModelTestingClass : NodeModel
        {
            public bool UpdateValueCoreBool(UpdateValueParams uvParams) => 
                UpdateValueCore(uvParams);

            public NodeMigrationData MigrateToDsFunctionNoAssembly(NodeMigrationData nmData, string name, string funcName) => 
                MigrateToDsFunction(nmData, name, funcName);

            public NodeMigrationData MigrateToDsFunctionAssembly(NodeMigrationData nmData, string assembly, string name, string funcName) => 
                MigrateToDsFunction(nmData, assembly, name, funcName);

            public NodeMigrationData MigrateToDsVarArgFunctionNMData(NodeMigrationData data, string assembly, string name, string funcName) => 
                MigrateToDsVarArgFunction(data, assembly, name, funcName);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateValueCoreTest()
        {
            var nodeModel = new NodeModelTestingClass();

            nodeModel.InPorts = new ObservableCollection<PortModel>
            {
                new PortModel("Port1", "Tooltip1"),
                new PortModel("Port2", "Tooltip2"),
                new PortModel("Port3", "Tooltip3")
            };

            //case "UsingDefaultValue"
            var param = new UpdateValueParams("UsingDefaultValue", "true;true;false");
            Assert.IsTrue(nodeModel.UpdateValueCoreBool(param));

            param = new UpdateValueParams("UsingDefaultValue", null);
            Assert.IsTrue(nodeModel.UpdateValueCoreBool(param));

            //case "KeepListStructure"
            param = new UpdateValueParams("KeepListStructure", "1:true");
            nodeModel.InPorts[0].KeepListStructure = true;

            Assert.IsTrue(nodeModel.UpdateValueCoreBool(param));
        }
    }
}