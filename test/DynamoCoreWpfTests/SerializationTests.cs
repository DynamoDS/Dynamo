﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

using DoubleSlider = CoreNodeModels.Input.DoubleSlider;
using Dynamo.Events;
using Dynamo.Models;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Engine;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.Watch3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dynamo.Utilities;

namespace DynamoCoreWpfTests
{
    internal class SerializationTests : DynamoViewModelUnitTest
    {
        [Test]
        [Category("UnitTests")]
        public void TestBasicAttributes()
        {
            var sumNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+")) { X = 400, Y = 100 };

            //Assert inital values
            Assert.AreEqual(400, sumNode.X);
            Assert.AreEqual(100, sumNode.Y);
            Assert.AreEqual("+", sumNode.Name);
            Assert.AreEqual(LacingStrategy.Auto, sumNode.ArgumentLacing);
            Assert.AreEqual(true, sumNode.IsVisible);
            Assert.AreEqual(true, sumNode.IsUpstreamVisible);
            Assert.AreEqual(ElementState.Dead, sumNode.State);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = sumNode.Serialize(xmlDoc, SaveContext.Undo);
            sumNode.X = 250;
            sumNode.Y = 0;
            sumNode.Name = "TestNode";
            sumNode.UpdateValue(new UpdateValueParams("ArgumentLacing", "CrossProduct"));
            sumNode.UpdateValue(new UpdateValueParams("IsVisible", "false"));
            sumNode.UpdateValue(new UpdateValueParams("IsUpstreamVisible", "false"));
            sumNode.State = ElementState.Active;

            //Assert New Changes
            Assert.AreEqual(250, sumNode.X);
            Assert.AreEqual(0, sumNode.Y);
            Assert.AreEqual("TestNode", sumNode.Name);
            Assert.AreEqual(LacingStrategy.CrossProduct, sumNode.ArgumentLacing);
            Assert.AreEqual(false, sumNode.IsVisible);
            Assert.AreEqual(false, sumNode.IsUpstreamVisible);
            Assert.AreEqual(ElementState.Active, sumNode.State);

            //Deserialize and Assert Old values
            sumNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, sumNode.X);
            Assert.AreEqual(100, sumNode.Y);
            Assert.AreEqual("+", sumNode.Name);
            Assert.AreEqual(LacingStrategy.Auto, sumNode.ArgumentLacing);
            Assert.AreEqual(true, sumNode.IsVisible);
            Assert.AreEqual(true, sumNode.IsUpstreamVisible);
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

            //Deserialize and aasert old values
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
            var element = dummyNode.Serialize(xmlDocument, SaveContext.File);

            // Dummy node should be serialized to its original node
            Assert.AreEqual(element.Name, "Dynamo.Nodes.DSFunction");
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

        private TimeSpan lastExecutionDuration = new TimeSpan();
        private Dictionary<Guid, string> modelsGuidToIdMap = new Dictionary<Guid, string>();

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private void DoWorkspaceOpenAndCompareView(string filePath, string dirName,
           Func<DynamoViewModel, string, string> saveFunction,
           Action<WorkspaceViewComparisonData, WorkspaceViewComparisonData> workspaceViewCompareFunction,
           Action<WorkspaceViewComparisonData, string, TimeSpan,Dictionary<Guid,string>> workspaceViewDataSaveFunction)
        {
            var openPath = filePath;

            if (Dynamo.Tests.SerializationTests.bannedTests.Any(t => filePath.Contains(t)))
            {
                Assert.Inconclusive("Skipping test known to kill the test framework...");
            }

            OpenModel(openPath);

            var model = this.ViewModel.Model;
            var ws1 = ViewModel.CurrentSpaceViewModel;

            ws1.Model.Description = "TestDescription";

            var dummyNodes = ws1.Nodes.Select(x => x.NodeModel).Where(n => n is DummyNode);
            if (dummyNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains dummy nodes for: " + string.Join(",", dummyNodes.Select(n => n.Name).ToArray()));
            }

            var cbnErrorNodes = ws1.Nodes.Where(n => n.NodeModel is CodeBlockNodeModel && n.NodeModel.State == ElementState.Error);
            if (cbnErrorNodes.Any())
            {
                Assert.Inconclusive("The Workspace contains code block nodes in error state due to which rest " +
                                    "of the graph will not execute; skipping test ...");
            }

            if (((HomeWorkspaceModel)ws1.Model).RunSettings.RunType == RunType.Manual)
            {
                RunCurrentModel();
            }

            var wcd1 = new WorkspaceViewComparisonData(ws1, model.EngineController);

            var dirPath = Path.Combine(Path.GetTempPath(), dirName);
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            var fi = new FileInfo(filePath);
            var filePathBase = dirPath + @"\" + Path.GetFileNameWithoutExtension(fi.Name);

            //no longer do this, as its done in model serialization tests.
            //ConvertCurrentWorkspaceToDesignScriptAndSave(filePathBase);

            string json = saveFunction(this.ViewModel, filePathBase);

            workspaceViewDataSaveFunction(wcd1, filePathBase, lastExecutionDuration,this.modelsGuidToIdMap);

            lastExecutionDuration = new TimeSpan();

            //make sure we're opening the json here
            this.ViewModel.OpenCommand.Execute(filePathBase + ".dyn");
            var ws2 = ViewModel.CurrentSpaceViewModel;
            Assert.NotNull(ws2);

            dummyNodes = ws2.Nodes.Select(x => x.NodeModel).Where(n => n is DummyNode);
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

        private static string ConvertCurrentWorkspaceViewToJsonAndSave(DynamoViewModel viewModel, string filePathBase)
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

            var jsonPath = filePathBase + ".dyn";
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
            File.WriteAllText(jsonPath, jo.ToString());

            return jo.ToString();
        }

        private string ConvertCurrentWorkspaceViewToNonGuidJsonAndSave(DynamoViewModel viewModel, string filePathBase)
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

            var tempPath = Path.GetTempPath();
            var jsonFolder = Path.Combine(tempPath, jsonNonGuidFolderName);

            if (!System.IO.Directory.Exists(jsonFolder))
            {
                System.IO.Directory.CreateDirectory(jsonFolder);
            }

            var jsonPath = filePathBase + ".dyn";
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
        [Test, TestCaseSource("FindWorkspaces")]
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
        [Test, TestCaseSource("FindWorkspaces")]
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
        public void AllTypesSerialize()
        {
            var customNodeTestPath = Path.Combine(TestDirectory, @"core\serialization\serialization.dyn");
            DoWorkspaceOpenAndCompareView(customNodeTestPath,
                jsonFolderName,
                ConvertCurrentWorkspaceViewToJsonAndSave,
                CompareWorkspaceViews,
                serializationTestUtils.SaveWorkspaceComparisonData);
        }

        public object[] FindWorkspaces()
        {
            var di = new DirectoryInfo(TestDirectory);
            var fis = di.GetFiles("*.dyn", SearchOption.AllDirectories);
            return fis.Select(fi => fi.FullName).ToArray();
        }


        internal class NodeViewComparisonData
        {
            public string ID { get; set; }
            public bool ShowGeometry { get; set; }
            public string Name { get; set; }
            public bool Excluded { get; set; }
            public double X { get; set; }
            public double Y { get; set; }

            public override bool Equals(object obj)
            {
                var other = (obj as NodeViewComparisonData);
                return ID == other.ID &&
                    other.ShowGeometry == this.ShowGeometry &&
                    other.Name == this.Name &&
                    other.Excluded == this.Excluded &&
                    Math.Abs(other.X - this.X) < .0001 &&
                    Math.Abs(other.Y - this.Y) < .0001;
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
                        TextBlockHeight = annotation.AnnotationModel.TextBlockHeight
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