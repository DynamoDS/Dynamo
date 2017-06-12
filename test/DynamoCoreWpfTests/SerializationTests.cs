using System;
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
}
