using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using Dynamo.Selection;
using Dynamo.Utilities;
using NUnit.Framework;
using Revit.Elements;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the NodeModel class
    /// </summary>
    [TestFixture]
    public class NodeModelTests : DynamoModelTestBase
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

        [Test]
        [Category("UnitTests")]
        public void SelectUpstreamNeighborsTest()
        {
            //Open and run the workspace
            var listTestFolder = Path.Combine(TestDirectory, "core");
            var testFilePath = Path.Combine(listTestFolder, "transpose.dyn");

            RunModel(testFilePath);

            //Select the range node in the transponse.dyn file
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("750720f0-a263-431d-86a5-52d622346eac");

            var countBefore = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(0, countBefore);

            //Run the method and assert that nodes upstream are selected
            node.SelectUpstreamNeighbours();

            var nodesSelected = DynamoSelection.Instance.Selection.Select(s=> s as NodeModel);

            var countAfter = nodesSelected.Count();
            Assert.AreEqual(6, countAfter);

            // Node GUIDs that should be selected
            string[] shouldBeSelected = {
                //Range node
                "f617749c-cee7-4c0a-823b-17c6fce1d977",
                //Number node
                "4831c3e1-6d21-4949-9592-03ee6e73c1e5",
                //Number node
                "7a840602-87d2-421b-9377-c3f4c4af143c",
                //Number node
                "83685125-12bf-4c61-aa9d-6dcb97fd280d",
                // + Node 
                "a4761902-905d-4466-a515-d52dadb1a0e7",
                //Number node
                "6f88b044-5447-4586-a53f-fd36dce6e06b"
            };

            // Range node selected
            for (int i = 0; i < shouldBeSelected.Length; i++)
                Assert.That(nodesSelected.Any(x => String.Equals(x.GUID.ToString(), shouldBeSelected[i])));
        }


        [Test]
        [Category("UnitTests")]
        public void SelectDownstreamNeighborsTest()
        {
            //Open and run the workspace
            var listTestFolder = Path.Combine(TestDirectory, "core");
            var testFilePath = Path.Combine(listTestFolder, "transpose.dyn");

            RunModel(testFilePath);

            //Select the range node in the transponse.dyn file
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("750720f0-a263-431d-86a5-52d622346eac");

            var countBefore = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(0, countBefore);

            //Run the method and assert whether more nodes were selected
            node.SelectDownstreamNeighbours();

            var nodesSelected = DynamoSelection.Instance.Selection.Select(s => s as NodeModel);
            var guids = nodesSelected.Select(n => n.GUID.ToString()).ToArray();
            var countAfter = nodesSelected.Count();
            Assert.AreEqual(3, countAfter);

            // Node GUIDs that should be selected
            string[] shouldBeSelected = {
                //Watch node
                "d528700d-73df-4ee8-a6b5-a24749ece16b",
                //List.Transpose node
                "31867b34-5da8-4a50-903e-e8a7f352fa5d",
                //Watch node
                "41d8a597-35c5-4056-857f-804790571e4c",
            };

            // Range node selected
            for (int i = 0; i < shouldBeSelected.Length; i++)
                Assert.That(nodesSelected.Any(x => String.Equals(x.GUID.ToString(), shouldBeSelected[i])));

        }

        [Test]
        [Category("UnitTests")]
        public void SelectDownstreamAndUpstreamNeighborsTest()
        {
            //Open and run the workspace
            var listTestFolder = Path.Combine(TestDirectory, "core");
            var testFilePath = Path.Combine(listTestFolder, "transpose.dyn");

            RunModel(testFilePath);

            //Select the range node in the transponse.dyn file
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("750720f0-a263-431d-86a5-52d622346eac");

            var countBefore = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(0, countBefore);

            //Run the method and assert whether more nodes were selected
            node.SelectUpstreamAndDownstreamNeighbours();

            var nodesSelected = DynamoSelection.Instance.Selection.Select(s => s as NodeModel);
            var guids = nodesSelected.Select(n => n.GUID.ToString()).ToArray();
            var countAfter = nodesSelected.Count();
            Assert.AreEqual(9, countAfter);

            // Node GUIDs that should be selected
            string[] shouldBeSelected = {
                //Range node
                "f617749c-cee7-4c0a-823b-17c6fce1d977",
                //Number node
                "4831c3e1-6d21-4949-9592-03ee6e73c1e5",
                //Number node
                "7a840602-87d2-421b-9377-c3f4c4af143c",
                //Number node
                "83685125-12bf-4c61-aa9d-6dcb97fd280d",
                // + Node 
                "a4761902-905d-4466-a515-d52dadb1a0e7",
                //Number node
                "6f88b044-5447-4586-a53f-fd36dce6e06b",
                //Watch node
                "d528700d-73df-4ee8-a6b5-a24749ece16b",
                //List.Transpose node
                "31867b34-5da8-4a50-903e-e8a7f352fa5d",
                //Watch node
                "41d8a597-35c5-4056-857f-804790571e4c",
            };

            // Range node selected
            for (int i = 0; i < shouldBeSelected.Length; i++)
                Assert.That(nodesSelected.Any(x => String.Equals(x.GUID.ToString(), shouldBeSelected[i])));
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateValueCoreTest()
        {
            var nodeModel = new NodeModelTestingClass();

            nodeModel.InPorts.AddRange(new ObservableCollection<PortModel>
            {
                new PortModel("Port1", "Tooltip1"),
                new PortModel("Port2", "Tooltip2"),
                new PortModel("Port3", "Tooltip3")
            });

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

    /// <summary>
    /// Class containing tests for some of the migration methods of the NodeModel class
    /// </summary>
    [TestFixture]
    public class NodeModelMigrationTests : DynamoModelTestBase
    {
        private NodeModelTestingClass nodeModel;
        private NodeMigrationData migrationDataTest;

        [SetUp]
        public void Init()
        {
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(documentDynPath);

            migrationDataTest = new NodeMigrationData(xmlDoc);

            XmlElement dsFunctionNode = (XmlElement)xmlDoc.SelectSingleNode("//Dynamo.Graph.Nodes.ZeroTouch.DSFunction");
            migrationDataTest.AppendNode(dsFunctionNode);

            nodeModel = new NodeModelTestingClass();
        }

        [Test]
        [Category("UnitTests")]
        public void MigrateToDsFunctionNoAssemblyTest()
        {
            var result = nodeModel.MigrateToDsFunctionNoAssembly(migrationDataTest, "nickname", "function");

            var assembly = result.MigratedNodes.First().Attributes["assembly"].Value;
            var nickname = result.MigratedNodes.First().Attributes["nickname"].Value;
            var function = result.MigratedNodes.First().Attributes["function"].Value;

            Assert.AreEqual(assembly, "");
            Assert.AreEqual(nickname, "nickname");
            Assert.AreEqual(function, "function");
        }

        [Test]
        [Category("UnitTests")]
        public void MigrateToDsFunctionWithAssemblyTest()
        {
            var result = nodeModel.MigrateToDsFunctionAssembly(migrationDataTest, "assembly", "nickname", "function");

            var assembly = result.MigratedNodes.First().Attributes["assembly"].Value;
            var nickname = result.MigratedNodes.First().Attributes["nickname"].Value;
            var function = result.MigratedNodes.First().Attributes["function"].Value;

            Assert.AreEqual(assembly, "assembly");
            Assert.AreEqual(nickname, "nickname");
            Assert.AreEqual(function, "function");
        }

        [Test]
        [Category("UnitTests")]
        public void MigrateToDsVarArgFunctionTest()
        {
            var result = nodeModel.MigrateToDsVarArgFunctionNMData(migrationDataTest, "assembly", "nickname", "function");

            var assembly = result.MigratedNodes.First().Attributes["assembly"].Value;
            var nickname = result.MigratedNodes.First().Attributes["nickname"].Value;
            var function = result.MigratedNodes.First().Attributes["function"].Value;

            Assert.AreEqual(assembly, "assembly");
            Assert.AreEqual(nickname, "nickname");
            Assert.AreEqual(function, "function"); 
        }
    }

    /// <summary>
    /// Class created in order to test protected methods in the NodeModel parent
    /// </summary>
    internal class NodeModelTestingClass : NodeModel
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
}
