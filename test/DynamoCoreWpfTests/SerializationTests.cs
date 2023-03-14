using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Engine;
using Dynamo.Events;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Tests;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestUINodes;
using DoubleSlider = CoreNodeModels.Input.DoubleSlider;

namespace DynamoCoreWpfTests
{
    internal class SerializationTests : DynamoViewModelUnitTest
    {

        [Test]
        [Category("UnitTests")]
        public void TestBasicAttributes()
        {
            var sumNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+")) { X = 400, Y = 100 };

            //Assert initial values
            Assert.AreEqual(400, sumNode.X);
            Assert.AreEqual(100, sumNode.Y);
            Assert.AreEqual("+", sumNode.Name);
            Assert.AreEqual(LacingStrategy.Auto, sumNode.ArgumentLacing);
            Assert.AreEqual(true, sumNode.IsVisible);
            Assert.AreEqual(ElementState.Dead, sumNode.State);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = sumNode.Serialize(xmlDoc, SaveContext.Undo);
            sumNode.X = 250;
            sumNode.Y = 0;
            sumNode.Name = "TestNode";
            sumNode.UpdateValue(new UpdateValueParams("ArgumentLacing", "CrossProduct"));
            sumNode.UpdateValue(new UpdateValueParams("IsVisible", "false"));
            sumNode.State = ElementState.Active;

            //Assert New Changes
            Assert.AreEqual(250, sumNode.X);
            Assert.AreEqual(0, sumNode.Y);
            Assert.AreEqual("TestNode", sumNode.Name);
            Assert.AreEqual(LacingStrategy.CrossProduct, sumNode.ArgumentLacing);
            Assert.AreEqual(false, sumNode.IsVisible);
            Assert.AreEqual(ElementState.Active, sumNode.State);

            //Deserialize and Assert Old values
            sumNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, sumNode.X);
            Assert.AreEqual(100, sumNode.Y);
            Assert.AreEqual("+", sumNode.Name);
            Assert.AreEqual(LacingStrategy.Auto, sumNode.ArgumentLacing);
            Assert.AreEqual(true, sumNode.IsVisible);
            Assert.AreEqual(ElementState.Dead, sumNode.State);
        }

        [Test]
        [Category("UnitTests")]
        public void TestDoubleInput()
        {
            var numNode = new DoubleInput { Value = "0.0", X = 400 };
            //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, numNode.X);
            Assert.AreEqual("0.0", numNode.Value);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = numNode.Serialize(xmlDoc, SaveContext.Undo);
            numNode.X = 250;
            numNode.Value = "4";

            //Assert new changes
            Assert.AreEqual(250, numNode.X);
            Assert.AreEqual("4", numNode.Value);

            //Deserialize and assert old values
            numNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, numNode.X);
            Assert.AreEqual("0.0", numNode.Value);
        }

        [Test]
        [Category("UnitTests")]
        public void TestDoubleSliderInput()
        {
            var numNode = new DoubleSlider { X = 400, Value = 50.0, Max = 100.0, Min = 0.0 };

            //To check if NodeModel base Serialization method is being called
            //To check if Double class's Serialization methods work

            //Assert initial values
            Assert.AreEqual(400, numNode.X);
            Assert.AreEqual(50.0, numNode.Value);
            Assert.AreEqual(0.0, numNode.Min);
            Assert.AreEqual(100.0, numNode.Max);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = numNode.Serialize(xmlDoc, SaveContext.Undo);
            numNode.X = 250;
            numNode.Value = 4.0;
            numNode.Max = 189.0;
            numNode.Min = 2.0;

            //Assert new changes
            Assert.AreEqual(250, numNode.X);
            Assert.AreEqual(4.0, numNode.Value);
            Assert.AreEqual(2.0, numNode.Min);
            Assert.AreEqual(189.0, numNode.Max);

            //Deserialize and aasert old values
            numNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, numNode.X);
            Assert.AreEqual(50.0, numNode.Value);
            Assert.AreEqual(0.0, numNode.Min);
            Assert.AreEqual(100.0, numNode.Max);
        }

        [Test]
        [Category("UnitTests")]
        public void TestBool()
        {
            var boolNode = new BoolSelector { Value = false, X = 400 };

            //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, boolNode.X);
            Assert.AreEqual(false, boolNode.Value);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = boolNode.Serialize(xmlDoc, SaveContext.Undo);
            boolNode.X = 250;
            boolNode.Value = true;

            //Assert new changes
            Assert.AreEqual(250, boolNode.X);
            Assert.AreEqual(true, boolNode.Value);

            //Deserialize and assert old values
            boolNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, boolNode.X);
            Assert.AreEqual(false, boolNode.Value);
        }

        [Test]
        [Category("UnitTests")]
        public void TestStringInput()
        {
            var strNode = new StringInput { Value = "Enter", X = 400 };
            //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, strNode.X);
            Assert.AreEqual("Enter", strNode.Value);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = strNode.Serialize(xmlDoc, SaveContext.Undo);
            strNode.X = 250;
            strNode.Value = "Exit";

            //Assert new changes
            Assert.AreEqual(250, strNode.X);
            Assert.AreEqual("Exit", strNode.Value);

            //Deserialize and aasert old values
            strNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, strNode.X);
            Assert.AreEqual("Enter", strNode.Value);
        }

        [Test]
        [Category("UnitTests")]
        public void TestStringFileName()
        {
            /*
            // "StringDirectory" class validates the directory name, so here we use one that we 
            // know for sure exists so the validation process won't turn it into empty string.
            var validFilePath = Assembly.GetExecutingAssembly().Location;
            var validDirectoryName = Path.GetDirectoryName(validFilePath);

            var strNode = new StringDirectory();
            strNode.Value = validDirectoryName;
            strNode.X = 400; //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, strNode.X);
            Assert.AreEqual(validDirectoryName, strNode.Value);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = strNode.Serialize(xmlDoc, SaveContext.Undo);
            strNode.X = 250;
            strNode.Value = "Invalid file path";

            //Assert new changes
            Assert.AreEqual(250, strNode.X);
            Assert.AreEqual("Invalid file path", strNode.Value);

            //Deserialize and aasert old values
            strNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, strNode.X);
            Assert.AreEqual(validDirectoryName, strNode.Value);
*/
            Assert.Inconclusive("Porting : StringDirectory");

        }

        [Test]
        public void TestVariableInput()
        {
            /*
            var listNode = new Dynamo.Nodes.NewList();
            listNode.X = 400; //To check if base Serialization method is being called
            listNode.InPortData.Add(new PortData("index 1", "Item Index #1", typeof(object)));
            listNode.InPortData.Add(new PortData("index 2", "Item Index #2", typeof(object)));

            //Assert initial values
            Assert.AreEqual(400, listNode.X);
            Assert.AreEqual(3, listNode.InPortData.Count);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = listNode.Serialize(xmlDoc, SaveContext.Undo);
            listNode.X = 250;
            listNode.InPortData.RemoveAt(listNode.InPortData.Count - 1);

            //Assert new changes
            Assert.AreEqual(250, listNode.X);
            Assert.AreEqual(2, listNode.InPortData.Count);

            //Deserialize and aasert old values
            listNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, listNode.X);
            Assert.AreEqual(3, listNode.InPortData.Count);
            Assert.AreEqual("index 2", listNode.InPortData.ElementAt(2).Name);
*/
            Assert.Inconclusive("Porting : NewList");

        }


        [Test]
        [Category("UnitTests")]
        public void TestFormula()
        {
            var formulaNode = new Formula { FormulaString = "x+y", X = 400 };

            //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, formulaNode.X);
            Assert.AreEqual("x+y", formulaNode.FormulaString);
            Assert.AreEqual(2, formulaNode.InPorts.Count);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = formulaNode.Serialize(xmlDoc, SaveContext.Undo);
            formulaNode.X = 250;
            formulaNode.FormulaString = "x+y+z";

            //Assert new changes
            Assert.AreEqual(250, formulaNode.X);
            Assert.AreEqual(3, formulaNode.InPorts.Count);
            Assert.AreEqual("x+y+z", formulaNode.FormulaString);

            //Deserialize and aasert old values
            formulaNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, formulaNode.X);
            Assert.AreEqual("x+y", formulaNode.FormulaString);
            Assert.AreEqual(2, formulaNode.InPorts.Count);
        }

        [Test]
        public void TestFunctionNode()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_serialization\");
            string openPath = Path.Combine(examplePath, "graph function.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            ViewModel.HomeSpace.Run();
            System.Threading.Thread.Sleep(500);

            // check if the node is loaded
            //Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count());

            var graphNode = model.CurrentWorkspace.NodeFromWorkspace<Function>("9c8c2279-6f59-417c-8218-3b337230bd99");
            //var graphNode = (Function)model.Nodes.First(x => x is Function);

            //Assert initial values
            Assert.AreEqual(534.75, graphNode.X);
            Assert.AreEqual("07e6b150-d902-4abb-8103-79193552eee7", graphNode.Definition.FunctionId.ToString());
            Assert.AreEqual("GraphFunction", graphNode.Name);
            Assert.AreEqual(4, graphNode.InPorts.Count);
            Assert.AreEqual("y", graphNode.InPorts[3].Name);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = graphNode.Serialize(xmlDoc, SaveContext.Undo);
            graphNode.X = 250;
            graphNode.Name = "NewNode";
            graphNode.InPorts.RemoveAt(graphNode.InPorts.Count - 1);

            //Assert new changes
            Assert.AreEqual(250, graphNode.X);
            Assert.AreEqual(3, graphNode.InPorts.Count);
            Assert.AreEqual("NewNode", graphNode.Name);

            //Deserialize and aasert old values
            graphNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(534.75, graphNode.X);
            Assert.AreEqual(4, graphNode.InPorts.Count);
            Assert.AreEqual("GraphFunction", graphNode.Name);
            Assert.AreEqual("y", graphNode.InPorts[3].Name);
        }

        [Test]
        public void NodeUserDescriptionTest()
        {
            // Arrange
            var userDescription = "Some description set by user...";
            var testFile = Path.Combine(TestDirectory, @"core\serialization\NodeUserDescriptionDeserilizationTest.dyn");         

            OpenModel(testFile);

            // Act
            // Stage 1: Serialize the workspace view.
            var jobject1 = JObject.Parse(ViewModel.CurrentSpaceViewModel.ToJson());
            var userDescriptionBefore = jobject1["NodeViews"].FirstOrDefault()["UserDescription"];

            // Stage 2: set UserDescription
            var nodeViewModel = this.ViewModel.CurrentSpaceViewModel.Nodes.First();
            nodeViewModel.UserDescription = userDescription;

            // Stage 3: Serialize the workspace view again to make sure UserDescription is now serialized.
            var jobject2 = JToken.Parse(ViewModel.CurrentSpaceViewModel.ToJson());
            var userDescriptionAfter = jobject2["NodeViews"].FirstOrDefault()["UserDescription"];

            // Assert
            Assert.That(nodeViewModel.UserDescription == userDescription);
            Assert.That(nodeViewModel.UserDescription == this.ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault().UserDescription);
            Assert.That(userDescriptionBefore is null);
            Assert.AreNotEqual(userDescriptionBefore, userDescriptionAfter);
            Assert.IsTrue(userDescriptionAfter.ToString() == userDescription);
        }

        [Test]
        public void TestDummyNodeInternals00()
        {
            var folder = Path.Combine(TestDirectory, @"core\dummy_node\");
            ViewModel.OpenCommand.Execute(Path.Combine(folder, "DummyNodeSample.dyn"));

            var workspace = ViewModel.Model.CurrentWorkspace;
            var dummyNode = workspace.NodeFromWorkspace<DummyNode>(
                Guid.Parse("37bffbb9-3438-4c6c-81d6-7b41b5fb5b87"));

            Assert.IsNotNull(dummyNode);

            // Ensure all properties are loaded from file.
            Assert.AreEqual("Point.ByLuck", dummyNode.LegacyNodeName);
            Assert.AreEqual(3, dummyNode.InputCount);
            Assert.AreEqual(2, dummyNode.OutputCount);

            // Ensure all properties updated data members accordingly.
            Assert.AreEqual("Point.ByLuck", dummyNode.Name);
            Assert.AreEqual(3, dummyNode.InPorts.Count);
            Assert.AreEqual(2, dummyNode.OutPorts.Count);
        }

        [Test]
        public void TestDummyNodeInternals01()
        {
            var folder = Path.Combine(TestDirectory, @"core\dummy_node\");
            ViewModel.OpenCommand.Execute(Path.Combine(folder, "DummyNodeSample.dyn"));

            var workspace = ViewModel.Model.CurrentWorkspace;
            var dummyNode = workspace.NodeFromWorkspace<DummyNode>(
                Guid.Parse("37bffbb9-3438-4c6c-81d6-7b41b5fb5b87"));

            Assert.IsNotNull(dummyNode);
            Assert.AreEqual(3, dummyNode.InPorts.Count);
            Assert.AreEqual(2, dummyNode.OutPorts.Count);

            var xmlDocument = new XmlDocument();
            var element = dummyNode.Serialize(xmlDocument, SaveContext.Undo);

            // Deserialize more than once should not cause ports to accumulate.
            dummyNode.Deserialize(element, SaveContext.Undo);
            dummyNode.Deserialize(element, SaveContext.Undo);
            dummyNode.Deserialize(element, SaveContext.Undo);

            Assert.AreEqual(3, dummyNode.InPorts.Count);
            Assert.AreEqual(2, dummyNode.OutPorts.Count);
        }

        [Test]
        public void TestDummyNodeSerialization()
        {
            var folder = Path.Combine(TestDirectory, @"core\dummy_node\");
            ViewModel.OpenCommand.Execute(Path.Combine(folder, "dummyNode.dyn"));

            var workspace = ViewModel.Model.CurrentWorkspace;
            var dummyNode = workspace.Nodes.OfType<DummyNode>().FirstOrDefault();

            Assert.IsNotNull(dummyNode);
            var xmlDocument = new XmlDocument();
            var element = dummyNode.Serialize(xmlDocument, SaveContext.Save);

            // Dummy node should be serialized to its original node
            Assert.AreEqual(element.Name, "Dynamo.Nodes.DSFunction");
        }

        [Test]
        public void NodeViewSerializationPropertiesTest()
        {
            var serializationNodeViewModelFile = Path.Combine(TestDirectory, @"core\serialization\serializationNodeViewModel.dyn");
            OpenModel(serializationNodeViewModelFile);

            var workSpaceJobject = JObject.Parse(ViewModel.CurrentSpaceViewModel.ToJson());
            JObject nodeViewModelJobject = JObject.Parse(workSpaceJobject["NodeViews"][0].ToString());

            Assert.AreEqual(8, nodeViewModelJobject.Properties().Count(), "The number of Serialized properties is not the expected");

            bool explicitOrder =
                nodeViewModelJobject.Properties().ElementAt(0).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.Id)) &&
                nodeViewModelJobject.Properties().ElementAt(1).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.Name)) &&
                nodeViewModelJobject.Properties().ElementAt(2).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.IsSetAsInput)) &&
                nodeViewModelJobject.Properties().ElementAt(3).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.IsSetAsOutput)) &&
                nodeViewModelJobject.Properties().ElementAt(4).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.IsFrozenExplicitly)) &&
                nodeViewModelJobject.Properties().ElementAt(5).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.IsVisible)) &&
                nodeViewModelJobject.Properties().ElementAt(6).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.X)) &&
                nodeViewModelJobject.Properties().ElementAt(7).Name == GetJsonPropertydName<NodeViewModel>(nameof(NodeViewModel.Y));

            Assert.IsTrue(explicitOrder, "The order of the properties is not the expected");
        }

        [Test]
        public void NodeModelSerializationPropertiesTest()
        {
            var openPath = Path.Combine(TestDirectory, @"core\serialization\serializationNodeModel.dyn");

            var jsonText = File.ReadAllText(openPath);
            var jobject1 = JObject.Parse(jsonText);
           
            JObject firstNodeModelObject = JObject.Parse(jobject1["Nodes"][0].ToString());

            bool firstNodeExplicitOrder =
                firstNodeModelObject.Properties().ElementAt(0).Name == "ConcreteType" &&
                firstNodeModelObject.Properties().ElementAt(1).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.GUID)) &&
                firstNodeModelObject.Properties().ElementAt(2).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.NodeType)) &&
                firstNodeModelObject.Properties().ElementAt(3).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.InPorts)) &&
                firstNodeModelObject.Properties().ElementAt(4).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.OutPorts)) &&                
                firstNodeModelObject.Properties().ElementAt(5).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.ArgumentLacing)) &&
                firstNodeModelObject.Properties().ElementAt(6).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.Description)) &&
                firstNodeModelObject.Properties().ElementAt(7).Name == GetJsonPropertydName<CodeBlockNodeModel>(nameof(CodeBlockNodeModel.Code));

            Assert.IsTrue(firstNodeExplicitOrder, "The first serialized Node doesn't have the expected order");

            JObject secondNodeModelObject = JObject.Parse(jobject1["Nodes"][1].ToString());

            bool secondNodeExplicitOrder =
                secondNodeModelObject.Properties().ElementAt(0).Name == "ConcreteType" &&
                secondNodeModelObject.Properties().ElementAt(1).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.GUID)) &&
                secondNodeModelObject.Properties().ElementAt(2).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.NodeType)) &&
                secondNodeModelObject.Properties().ElementAt(3).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.InPorts)) &&
                secondNodeModelObject.Properties().ElementAt(4).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.OutPorts)) &&
                secondNodeModelObject.Properties().ElementAt(5).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.ArgumentLacing)) &&
                secondNodeModelObject.Properties().ElementAt(6).Name == GetJsonPropertydName<NodeModel>(nameof(NodeModel.Description)) &&
                secondNodeModelObject.Properties().ElementAt(7).Name == GetJsonPropertydName<FileSystemBrowser>(nameof(FileSystemBrowser.HintPath)) &&
                secondNodeModelObject.Properties().ElementAt(8).Name == GetJsonPropertydName<BasicInteractive<object>>(nameof(BasicInteractive<object>.Value));
            

            Assert.IsTrue(secondNodeExplicitOrder, "The second serialized Node doesn't have the expected order");
        }

        public string GetJsonPropertydName<T>(string propertyName)
        {
            System.Reflection.PropertyInfo t = typeof(T).GetProperty(propertyName);
            var attr = t.GetCustomAttributes(typeof(Newtonsoft.Json.JsonPropertyAttribute), true).FirstOrDefault() as Newtonsoft.Json.JsonPropertyAttribute;

            return attr.PropertyName ?? propertyName;
        }

        [Test]
        public void JSONisSameBeforeAndAfterSaveWithDummyNodes()
        {
            var testFileWithDummyNode = @"core\dummy_node\2080_JSONTESTCRASH undo_redo.dyn";
            var openPath = Path.Combine(TestDirectory, testFileWithDummyNode);

            var jsonText1 = File.ReadAllText(openPath);
            var jobject1 = JObject.Parse(jsonText1);

            // We need to replace the camera with default camera so it will match the deafult camera produced by the 
            // save without a real view below.
            jobject1["View"]["Camera"] = JToken.FromObject(new CameraData());
            jsonText1 = jobject1.ToString();
            jobject1 = JObject.Parse(jsonText1);
          
            OpenModel(openPath);
            Assert.AreEqual(1, ViewModel.CurrentSpace.Nodes.OfType<DummyNode>().Count());

            // Stage 1: Serialize the workspace.
            var jsonModel = ViewModel.Model.CurrentWorkspace.ToJson(ViewModel.Model.EngineController);

            // Stage 2: Add the View.
            var jobject2 = JObject.Parse(jsonModel);
            var token = JToken.Parse(ViewModel.CurrentSpaceViewModel.ToJson());
            jobject2.Add("View", token);

            // Re-saving the file will update the version number (which can be expected)
            // Setting the version numbers to be equal to stop the deep compare from failing
            jobject2["View"]["Dynamo"]["Version"] = jobject1["View"]["Dynamo"]["Version"];

            // Ignoring the ExtensionWorkspaceData property as this is added after the re-save,
            // this will cause a difference between jobject1 and jobject2 if it is not ignored.
            // Same thing goes for the Linting property...
            // We also need to ignore the new IsCollapsed property on ViewModelBase
            jobject2.Remove(WorkspaceReadConverter.EXTENSION_WORKSPACE_DATA);
            jobject2.Remove(LinterManagerConverter.LINTER_START_OBJECT_NAME);
            foreach(JObject item in jobject2["View"]["NodeViews"])
            {
                item.Remove(nameof(ViewModelBase.IsCollapsed));
            }

            var jsonText2 = jobject2.ToString();

            Console.WriteLine(jsonText1);
            Console.WriteLine(jsonText2);

            Assert.IsTrue(JToken.DeepEquals(jobject1, jobject2));
        }

        [Test]
        public void TestUndoRedoOnConnectedNodes()
        {
            ViewModel.OpenCommand.Execute(Path.Combine(TestDirectory, "core", "LacingTest.dyn"));
            var workspace = ViewModel.CurrentSpaceViewModel;

            Assert.IsFalse(workspace.SetArgumentLacingCommand.CanExecute(null));
            workspace.SelectAllCommand.Execute(null);
            Assert.IsTrue(workspace.SetArgumentLacingCommand.CanExecute(null));

            Assert.DoesNotThrow(() =>
            {
                workspace.SetArgumentLacingCommand.Execute(LacingStrategy.Longest.ToString());
            });
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }
    }

    class JSONSerializationTests : DynamoViewModelUnitTest
    {
        public static string jsonNonGuidFolderName = "jsonWithView_nonGuidIds";
        public static string jsonFolderName = "jsonWithView";

        private const string jsonStructuredFolderName = "DynamoCoreWPFTests";

        private TimeSpan lastExecutionDuration = new TimeSpan();
        private Dictionary<Guid, string> modelsGuidToIdMap = new Dictionary<Guid, string>();
        private const int MAXNUM_SERIALIZATIONTESTS_TOEXECUTE = 300;


        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private void DoWorkspaceOpen(string filePath)
        {
            if (Dynamo.Tests.SerializationTests.bannedTests.Any(t => filePath.Contains(t)))
            {
                Assert.Inconclusive("Skipping test known to kill the test framework...");
            }

            OpenModel(filePath);

            var model = this.ViewModel.Model;
            var workspace = ViewModel.CurrentSpaceViewModel;

            workspace.Model.Description = "TestDescription";

            var dummyNodes = workspace.Nodes.Select(x => x.NodeModel).Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            var cbnErrorNodes = workspace.Nodes.Where(n => n.NodeModel is CodeBlockNodeModel && n.NodeModel.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)workspace.Model).RunSettings.RunType == RunType.Manual)
            {
                RunCurrentModel();
            }
        }

        private void DoWorkspaceOpenAndCompareView(string filePath, string dirName,
           Func<DynamoViewModel, string, string> saveFunction,
           Action<WorkspaceViewComparisonData, WorkspaceViewComparisonData> workspaceViewCompareFunction,
           Action<WorkspaceViewComparisonData, string, TimeSpan,Dictionary<Guid,string>> workspaceViewDataSaveFunction)
        {
            var openPath = filePath;
            DoWorkspaceOpen(openPath);

            var ws1 = ViewModel.CurrentSpaceViewModel;
            var wcd1 = new WorkspaceViewComparisonData(ws1, ViewModel.Model.EngineController);

            var dirPath = Path.Combine(Path.GetTempPath(), dirName);
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            var fi = new FileInfo(filePath);
            var filePathBase = dirPath + @"\" + Path.GetFileNameWithoutExtension(fi.Name);

            //no longer do this, as its done in model serialization tests.
            //ConvertCurrentWorkspaceToDesignScriptAndSave(filePathBase);

            string json = saveFunction(this.ViewModel, filePath);

            // If "jsonWithView_nonGuidIds" test copy .data file to additional structured folder location
            if (dirName == jsonNonGuidFolderName)
            {
                // Get structured test path
                var testPath = filePath.Remove(0, SerializationTests.TestDirectory.Length);
                var tempPath = Path.GetTempPath();
                var fullPath = System.IO.Path.ChangeExtension(tempPath + jsonStructuredFolderName + testPath + fi.Extension, null);
                workspaceViewDataSaveFunction(wcd1, fullPath, lastExecutionDuration, this.modelsGuidToIdMap);
            }

            else
            {
                workspaceViewDataSaveFunction(wcd1, filePathBase, lastExecutionDuration, this.modelsGuidToIdMap);
            }

            lastExecutionDuration = new TimeSpan();

            // Open JSON .dyn or .dyf
            var fileExtension = Path.GetExtension(filePath);
            this.ViewModel.OpenCommand.Execute(filePathBase + fileExtension);

            var ws2 = ViewModel.CurrentSpaceViewModel;
            Assert.NotNull(ws2);

            var dummyNodes = ws2.Nodes.Select(x => x.NodeModel).Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            if (((HomeWorkspaceModel)ws2.Model).RunSettings.RunType == RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd2 = new WorkspaceViewComparisonData(ws2, this.ViewModel.EngineController);

            workspaceViewCompareFunction(wcd1, wcd2);

            //TODO remove this after we merge notes and annotations -
            //this is done here so we don't need to modify the workspaceComparison classes
            //and it's simple to remove soon.
            var index = 0;
            foreach(var noteView in ws1.Notes)
            {
                var matchingNote = ws2.Notes[index];
                Assert.IsTrue(noteView.Model.Text == matchingNote.Model.Text);
                Assert.Less(Math.Abs(noteView.Model.X - matchingNote.Model.X),0001);
                Assert.Less(Math.Abs(noteView.Model.Y - matchingNote.Model.Y), 0001);
                index = index + 1;
            }

        }

        /// <summary>
        /// Copy test file to specified folder while 
        /// maintaining original directory structure
        /// and assigning file path as workspace name.
        /// These are .dyn or .dyf files.
        /// </summary>
        /// <param name="filePath">original test file path</param>
        /// <param name="jo">test json object</param>
        private static void SaveJsonTempWithFolderStructure(string filePath, JObject jo, DynamoModel currentDynamoModel)
        {
            // Get all folder structure following "\\test"
            var expectedStructure = filePath.Remove(0, SerializationTests.TestDirectory.Length);
            var newWSName = expectedStructure.Replace("\\", "/");
            var extension = Path.GetExtension(filePath);

            // Update WS name to original test file path
            jo["Name"] = newWSName;

            if (extension == ".dyf")
            {
                // If .dyf file use the existing Uuid
                var customNodeWS = currentDynamoModel.CurrentWorkspace as CustomNodeWorkspaceModel;
                if(customNodeWS != null)
                {
                    jo["Uuid"] = customNodeWS.CustomNodeId;
                }
            }

            else
            {
                // If .dyn file update Uuid to be unique based on new WS name
                var nameBasedGuid = GuidUtility.Create(currentDynamoModel.CurrentWorkspace.Guid, newWSName);
                jo["Uuid"] = nameBasedGuid;
            }

            // Current test fileName
            var fileName = Path.GetFileName(filePath);

            // Get temp folder path
            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonStructuredFolderName);
            jsonFolder += Path.GetDirectoryName(expectedStructure);

            if (!System.IO.Directory.Exists(jsonFolder))
            {
                System.IO.Directory.CreateDirectory(jsonFolder);
            }

            // TODO add check to make sure a .dyn or .dyf with the same name does not exist
            // Combine directory with test file name
            var jsonPath = jsonFolder + "\\" + fileName;

            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }

            File.WriteAllText(jsonPath, jo.ToString());

            // Write DesignScript file
            string dsFileName = Path.GetFileNameWithoutExtension(fileName);

            // Determine if .dyn or .dyf
            // If .dyn and .dyf share common file name .ds and .data files is collide
            // To avoid this append _dyf to .data and .ds files for all .dyf files
            if (extension == ".dyf")
            {
                dsFileName += "_dyf";
            }

            string dsPath = jsonFolder + "\\" + dsFileName;
            serializationTestUtils.ConvertCurrentWorkspaceToDesignScriptAndSave(dsPath, currentDynamoModel);
        }

        private static string ConvertCurrentWorkspaceViewToJsonAndSave(DynamoViewModel viewModel, string filePath)
        {
            // Stage 1: Serialize the workspace.
            var jsonModel = viewModel.Model.CurrentWorkspace.ToJson(viewModel.Model.EngineController);

            // Stage 2: Add the View.
            var jo = JObject.Parse(jsonModel);
            var token = JToken.Parse(viewModel.CurrentSpaceViewModel.ToJson());
            jo.Add("View", token);

            Assert.IsNotNullOrEmpty(jsonModel);
            Assert.IsNotNullOrEmpty(jo.ToString());

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonFolderName);

            if (!System.IO.Directory.Exists(jsonFolder))
            {
                System.IO.Directory.CreateDirectory(jsonFolder);
            }

            var fileName = Path.GetFileName(filePath);
            var jsonPath = Path.Combine(jsonFolder, fileName);

            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, jo.ToString());

            return jo.ToString();
        }

        private string ConvertCurrentWorkspaceViewToNonGuidJsonAndSave(DynamoViewModel viewModel, string filePath)
        {
            // Stage 1: Serialize the workspace.
            var jsonModel = viewModel.Model.CurrentWorkspace.ToJson(viewModel.Model.EngineController);
            // Stage 2: Add the View.
            var jo = JObject.Parse(jsonModel);
            var token = JToken.Parse(viewModel.CurrentSpaceViewModel.ToJson());
            jo.Add("View", token);
            var json = jo.ToString();
            var model = viewModel.Model;

            json = serializationTestUtils.replaceModelIdsWithNonGuids(json, model.CurrentWorkspace ,modelsGuidToIdMap);

            Assert.IsNotNullOrEmpty(json);

            // Call structured copy function for CoGS testing, see QNTM-2973
            // Only called for CoreWPFTests nonGuids
            var nonGuidsJson = JObject.Parse(json);
            SaveJsonTempWithFolderStructure(filePath, nonGuidsJson, viewModel.Model);

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonNonGuidFolderName);

            if (!System.IO.Directory.Exists(jsonFolder))
            {
                System.IO.Directory.CreateDirectory(jsonFolder);
            }

            var fileName = Path.GetFileName(filePath);
            var jsonPath = Path.Combine(jsonFolder, fileName);

            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, json);

            return json;
        }

        private void CompareWorkspaceViews(WorkspaceViewComparisonData a, WorkspaceViewComparisonData b)
        {
            //first compare the model data
            serializationTestUtils.CompareWorkspaceModels(a, b, this.modelsGuidToIdMap);

            Assert.IsTrue(Math.Abs(a.X - b.X) < .00001, "The workspaces don't have the same X offset.");
            Assert.IsTrue(Math.Abs(a.X - b.X) < .00001, "The workspaces don't have the same Y offset.");
            Assert.IsTrue(Math.Abs(a.Zoom - b.Zoom) < .00001, "The workspaces don't have the same Zoom.");
            Assert.AreEqual(a.Camera, b.Camera);
            Assert.AreEqual(a.Guid, b.Guid);

            Assert.AreEqual(a.NodeViewCount, b.NodeViewCount, "The workspaces don't have the same number of node views.");
            Assert.AreEqual(a.ConnectorViewCount, b.ConnectorViewCount, "The workspaces don't have the same number of connector views.");

            Assert.AreEqual(a.AnnotationMap.Count, b.AnnotationMap.Count);

            foreach (var annotationKVP in a.AnnotationMap)
            {
                var dataA = a.AnnotationMap[annotationKVP.Key];
                var dataB = b.AnnotationMap[annotationKVP.Key];

                Assert.AreEqual(dataA, dataB);
            }

            foreach (var kvp in a.NodeViewDataMap)
            {
                var valueA = kvp.Value;
                var valueB = b.NodeViewDataMap[kvp.Key];
                Assert.AreEqual(valueA, valueB,
                string.Format("Node View Data:{0} value, {1} is not equal to {2}",
                a.NodeViewDataMap[kvp.Key].Name, valueA, valueB));
            }
        }

        private void CompareWorkspaceViewsDifferentGuids(WorkspaceViewComparisonData a, WorkspaceViewComparisonData b)
        {
            //first compare the model data
            serializationTestUtils.CompareWorkspacesDifferentGuids(a, b, this.modelsGuidToIdMap);

            Assert.IsTrue(Math.Abs(a.X - b.X) < .00001, "The workspaces don't have the same X offset.");
            Assert.IsTrue(Math.Abs(a.X - b.X) < .00001, "The workspaces don't have the same Y offset.");
            Assert.IsTrue(Math.Abs(a.Zoom - b.Zoom) < .00001, "The workspaces don't have the same Zoom.");
            Assert.AreEqual(a.Camera, b.Camera);
            Assert.AreEqual(a.Guid, b.Guid);

            Assert.AreEqual(a.NodeViewCount, b.NodeViewCount, "The workspaces don't have the same number of node views.");
            Assert.AreEqual(a.ConnectorViewCount, b.ConnectorViewCount, "The workspaces don't have the same number of connector views.");

            Assert.AreEqual(a.AnnotationMap.Count, b.AnnotationMap.Count);

            foreach (var annotationKVP in a.AnnotationMap)
            {
                var valueA = annotationKVP.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[annotationKVP.Key]);
                var valueB = b.AnnotationMap[newGuid];
                //set the id explicitly since we know it will have changed and should be this id.
                valueB.Id = valueA.Id.ToString();
                //check at least that number of referenced nodes is correct.
                Assert.AreEqual(valueB.Nodes.Count(), valueA.Nodes.Count());
                //ignore this list because all node ids will have changed.
                valueB.Nodes = valueA.Nodes;

                Assert.AreEqual(valueA, valueB);
            }

            foreach (var kvp in a.NodeViewDataMap)
            {
                var valueA = kvp.Value;
                //convert the old guid to the new guid
                var newGuid = GuidUtility.Create(GuidUtility.UrlNamespace, this.modelsGuidToIdMap[kvp.Key]);
                var valueB = b.NodeViewDataMap[newGuid];
                //set the id explicitly since we know it will have changed and should be this id.
                valueB.ID = valueA.ID.ToString();

                Assert.AreEqual(valueA, valueB,
                string.Format("Node View Data:{0} value, {1} is not equal to {2}",
                a.NodeViewDataMap[kvp.Key].Name, valueA, valueB));
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;

            //Clear Temp directory folders before start of the new serialization test run
            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonFolderName);
            var jsonNonGuidFolder = Path.Combine(tempPath,  jsonNonGuidFolderName);

            //Try and delete all the files from the previous run. 
            //If there's an error in deleting files, the tests should countinue
            if (System.IO.Directory.Exists(jsonFolder))
            {
                try
                {
                    Console.WriteLine("Deleting JsonWithView directory from temp");
                    System.IO.Directory.Delete(jsonFolder, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (System.IO.Directory.Exists(jsonNonGuidFolder))
            {
                try
                {
                    Console.WriteLine("Deleting JsonWithViewNonGuid directory from temp");
                    System.IO.Directory.Delete(jsonNonGuidFolder, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Dynamo.Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Dynamo.Session.ParameterKeys.LastExecutionDuration);
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces"), Category("JsonTestExclude")]
        public void SerializationTest(string filePath)
        {
            DoWorkspaceOpenAndCompareView(filePath,
               jsonFolderName,
                ConvertCurrentWorkspaceViewToJsonAndSave,
                CompareWorkspaceViews,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        /// <summary>
        /// This parameterized test finds all .dyn files in directories within
        /// the test directory, opens them and executes, then converts them to
        /// json and executes again, comparing the values from the two runs.
        /// This set of tests has slightly modified json where the id properties
        /// are altered when serialized to test deserialization of non-guid ids.
        /// </summary>
        /// <param name="filePath">The path to a .dyn file. This parameter is supplied
        /// by the test framework.</param>
        [Test, TestCaseSource("FindWorkspaces"), Category("JsonTestExclude")]
        public void SerializationNonGuidIdsTest(string filePath)
        {
            modelsGuidToIdMap.Clear();
            DoWorkspaceOpenAndCompareView(filePath,
               jsonNonGuidFolderName,
                ConvertCurrentWorkspaceViewToNonGuidJsonAndSave,
                CompareWorkspaceViewsDifferentGuids,
                serializationTestUtils.SaveWorkspaceComparisonDataWithNonGuidIds);
        }

        [Test]
        public void CustomNodeSerializationTest()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\CustomNodes\TestAdd.dyn");
            DoWorkspaceOpenAndCompareView(customNodeTestPath,
               jsonFolderName, 
                ConvertCurrentWorkspaceViewToJsonAndSave,
                CompareWorkspaceViews,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        [Test]
        public void NewCustomNodeSaveAndLoadPt1()
        {
            var funcguid = GuidUtility.Create(GuidUtility.UrlNamespace, "NewCustomNodeSaveAndLoad");
            //first create a new custom node.
            var ws = this.ViewModel.Model.CustomNodeManager.CreateCustomNode("testnode", "testcategory", "atest", funcguid);
            var outnode1 = new Output();
            outnode1.Symbol = "out1";
            var outnode2 = new Output();
            outnode2.Symbol = "out2";

            var numberNode = new DoubleInput();
            numberNode.Value = "5";
          

            ws.AddAndRegisterNode(numberNode);
            ws.AddAndRegisterNode(outnode1);
            ws.AddAndRegisterNode(outnode2);

            new ConnectorModel(numberNode.OutPorts.FirstOrDefault(), outnode1.InPorts.FirstOrDefault(), Guid.NewGuid());
            new ConnectorModel(numberNode.OutPorts.FirstOrDefault(), outnode2.InPorts.FirstOrDefault(), Guid.NewGuid());

            var saveDir = Path.Combine(Path.GetTempPath(), "NewCustomNodeSaveAndLoad");
            System.IO.Directory.CreateDirectory(saveDir);

            var savePath = Path.Combine(saveDir, "NewCustomNodeSaveAndLoad.dyf");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            //save it to a temp location so that we can safely load it in NewCustomNodeSaveAndLoadPt2
            ws.Save(savePath);

            //assert the filesaved
            Assert.IsTrue(File.Exists(savePath));
            Assert.IsFalse(string.IsNullOrEmpty(File.ReadAllText(savePath)));
        }

        [Test]
        public void NewCustomNodeSaveAndLoadPt2()
        {
            var newPaths = new List<string> { Path.Combine(Path.GetTempPath(), "NewCustomNodeSaveAndLoad") };
            ViewModel.Model.PreferenceSettings.CustomPackageFolders = newPaths;

            var loader = ViewModel.Model.GetPackageManagerExtension().PackageLoader;
            loader.LoadNewCustomNodesAndPackages(newPaths, ViewModel.Model.CustomNodeManager);
            // This unit test is a follow-up of NewCustomNodeSaveAndLoadPt1 test to make sure the newly created
            // custom node will be loaded once DynamoCore restarted
            var funcguid = GuidUtility.Create(GuidUtility.UrlNamespace, "NewCustomNodeSaveAndLoad");
            var functionnode =
                ViewModel.Model.CustomNodeManager.CreateCustomNodeInstance(funcguid, "testnode", true);
            Assert.IsTrue(functionnode.IsCustomFunction);
            Assert.IsFalse(functionnode.IsInErrorState);
            Assert.AreEqual(functionnode.OutPorts.Count, 2);

            ViewModel.CurrentSpace.AddAndRegisterNode(functionnode);
            var nodeingraph = ViewModel.CurrentSpace.Nodes.FirstOrDefault();
            Assert.NotNull(nodeingraph);
            Assert.IsTrue(nodeingraph.State == ElementState.Active);
            //remove custom node from definitions folder
            var savePath = Path.Combine(Path.GetTempPath(), "NewCustomNodeSaveAndLoad", "NewCustomNodeSaveAndLoad.dyf");
            File.Delete(savePath);
        }

        [Test]
        public void AllTypesSerialize()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            DoWorkspaceOpenAndCompareView(customNodeTestPath,
                jsonFolderName,
                ConvertCurrentWorkspaceViewToJsonAndSave,
                CompareWorkspaceViews,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        // This test checks that all notes are properly converted to annotations
        // when saving to JSON.
        [Test]
        public void NotesSerializeAsAnnotations()
        {
            var filePath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            DoWorkspaceOpen(filePath);
            var workspace = ViewModel.Model.CurrentWorkspace;

            var numXMLNotes = workspace.Notes.Count();
            var numXMLAnnotations = workspace.Annotations.Count();

            var view = JToken.Parse(ViewModel.CurrentSpaceViewModel.ToJson());
            var numJsonAnnotations = view["Annotations"].Count();

            Assert.AreEqual(numXMLNotes, 0);
            Assert.AreEqual(numXMLAnnotations, numJsonAnnotations);
        }

        [Test]
        public void DropDownsHaveCorrectInputDataTypes()
        {
            var dropnode = new EnumAsStringConcrete();
            var data = dropnode.InputData;
            Assert.AreEqual(NodeInputTypes.selectionInput, data.Type);
            Assert.AreEqual(NodeInputTypes.dropdownSelection, data.Type2);
        }
        [Test]
        public void SelectionNodesHaveCorrectInputDataTypes()
        {
            var selectNode = new SelectionConcrete(SelectionType.One, SelectionObjectType.None, "", "");
            var data = selectNode.InputData;
            Assert.AreEqual(NodeInputTypes.selectionInput, data.Type);
            Assert.AreEqual(NodeInputTypes.hostSelection, data.Type2);
        }
        [Test]
        public void SelectionNodeInputDataSerializationTest()
        {
            // Arrange
            var filePath = Path.Combine(TestDirectory, @"core\NodeInputOutputData\selectionNodeInputData.dyn");
            if (!File.Exists(filePath))
            {
                var savePath = Path.ChangeExtension(filePath, null);
                var selectionHelperMock = new Mock<IModelSelectionHelper<ModelBase>>(MockBehavior.Strict);
                var selectionNode = new SelectionConcrete(SelectionType.Many, SelectionObjectType.Element, "testMessage", "testPrefix", selectionHelperMock.Object);
                selectionNode.Name = "selectionTestName";
                selectionNode.IsSetAsInput = true;

                ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(selectionNode);
                ConvertCurrentWorkspaceViewToJsonAndSave(ViewModel, savePath);
            }

            // Act
            // Assert
            DoWorkspaceOpenAndCompareView(
                filePath,
                jsonFolderName,
                ConvertCurrentWorkspaceViewToJsonAndSave,
                //TODO(MJK) potentially just use compareworkspacedata if test fails.
                CompareWorkspaceViews,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(TestDirectory);
            var fis = new string[] { "*.dyn", "*.dyf" }
            .SelectMany(i => di.GetFiles(i, SearchOption.AllDirectories));
            return fis.Select(fi => fi.FullName).Take(MAXNUM_SERIALIZATIONTESTS_TOEXECUTE).ToArray();
        }


        internal class NodeViewComparisonData
        {
            public string ID { get; set; }
            public bool ShowGeometry { get; set; }
            public string Name { get; set; }
            public bool Excluded { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public bool IsInput { get; set; }
            public bool IsOutput { get; set; }

            public override bool Equals(object obj)
            {
                var other = (obj as NodeViewComparisonData);
                return ID == other.ID &&
                    other.ShowGeometry == this.ShowGeometry &&
                    other.Name == this.Name &&
                    other.Excluded == this.Excluded &&
                    Math.Abs(other.X - this.X) < .0001 &&
                    Math.Abs(other.Y - this.Y) < .0001 &&
                    other.IsInput == this.IsInput &&
                    other.IsOutput == this.IsOutput;
            }
        }

        private class WorkspaceViewComparisonData : serializationTestUtils.WorkspaceComparisonData
        {
            public int NodeViewCount { get; set; }
            public int ConnectorViewCount { get; set; }
            public Dictionary<Guid, NodeViewComparisonData> NodeViewDataMap { get; set; }
            public Dictionary<Guid, ExtraAnnotationViewInfo> AnnotationMap { get; set; }
            public CameraData Camera { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Zoom { get; set; }

            public WorkspaceViewComparisonData(WorkspaceViewModel workspaceView, EngineController controller):base(workspaceView.Model,controller)
            {
                NodeViewCount = workspaceView.Nodes.Count();
                ConnectorViewCount = workspaceView.Connectors.Count();
                NodeViewDataMap = new Dictionary<Guid, NodeViewComparisonData>();
                AnnotationMap = new Dictionary<Guid, ExtraAnnotationViewInfo>();

                foreach (var annotation in workspaceView.Annotations)
                {
                    AnnotationMap.Add(annotation.AnnotationModel.GUID, new ExtraAnnotationViewInfo
                    {
                        Background = annotation.Background.ToString(),
                        FontSize = annotation.FontSize,
                        Nodes = annotation.Nodes.Select(x => x.GUID.ToString()),
                        Title = annotation.AnnotationText,
                        Id = annotation.AnnotationModel.GUID.ToString(),
                        Left = annotation.Left,
                        Top = annotation.Top,
                        Width = annotation.Width,
                        Height = annotation.Height,
                        InitialTop = annotation.AnnotationModel.InitialTop,
                        TextBlockHeight = annotation.AnnotationModel.TextBlockHeight,
                        HasNestedGroups = annotation.AnnotationModel.HasNestedGroups
                    });
                }

                foreach (var n in workspaceView.Nodes)
                {
                    NodeViewDataMap.Add(n.NodeModel.GUID, new NodeViewComparisonData
                    {
                        ShowGeometry = n.IsVisible,
                        ID = n.NodeModel.GUID.ToString(),
                        Name = n.Name,
                        Excluded = n.IsFrozenExplicitly,
                        X = n.X,
                        Y = n.Y,
                        IsInput = n.IsSetAsInput,
                        IsOutput = n.IsSetAsOutput
                    });
                }

                X = workspaceView.X;
                Y = workspaceView.Y;
                Zoom = workspaceView.Zoom;
                Camera = workspaceView.Camera;
            }
        }
    }
}
