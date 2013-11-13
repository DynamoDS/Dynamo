using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class RecordedTests
    {
        private System.Random randomizer = null;

        // For access within test cases.
        private WorkspaceModel workspace = null;
        private WorkspaceViewModel workspaceViewModel = null;
        private DynamoController controller = null;

        [SetUp]
        public void Start()
        {
            // Fixed seed randomizer for predictability.
            randomizer = new System.Random(123456);
        }

        [TearDown]
        public void Exit()
        {
            this.controller = null;
        }

        [Test, RequiresSTA]
        public void TestCreateNodeCommand()
        {
            // Create the command in completely unpredictable states. These 
            // states should properly be serialized and deserialized across 
            // two instances of the same command.
            // 
            Guid nodeId = Guid.NewGuid();
            string name = randomizer.Next().ToString();
            double x = randomizer.NextDouble() * 1000;
            double y = randomizer.NextDouble() * 1000;
            bool defaultPos = randomizer.Next(2) == 0;
            bool transfPos = randomizer.Next(2) == 0;

            var cmdOne = new DynCmd.CreateNodeCommand(
                nodeId, name, x, y, defaultPos, transfPos);

            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.NodeId, cmdTwo.NodeId);
            Assert.AreEqual(cmdOne.NodeName, cmdTwo.NodeName);
            Assert.AreEqual(cmdOne.X, cmdTwo.X, 0.000001);
            Assert.AreEqual(cmdOne.Y, cmdTwo.Y, 0.000001);
            Assert.AreEqual(cmdOne.DefaultPosition, cmdTwo.DefaultPosition);
            Assert.AreEqual(cmdOne.TransformCoordinates, cmdTwo.TransformCoordinates);

            // A RecordableCommand should be created in "recording mode" by default, 
            // and only deserialized commands should be marked as "in playback mode".
            Assert.AreEqual(false, cmdOne.IsInPlaybackMode);
            Assert.AreEqual(true, cmdTwo.IsInPlaybackMode);
        }

        [Test, RequiresSTA]
        public void TestCreateNoteCommand()
        {
            // Create the command in completely unpredictable states. These 
            // states should properly be serialized and deserialized across 
            // two instances of the same command.
            // 
            Guid nodeId = Guid.NewGuid();
            string text = randomizer.Next().ToString();
            double x = randomizer.NextDouble() * 1000;
            double y = randomizer.NextDouble() * 1000;
            bool defaultPos = randomizer.Next(2) == 0;

            var cmdOne = new DynCmd.CreateNoteCommand(nodeId, text, x, y, defaultPos);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.NodeId, cmdTwo.NodeId);
            Assert.AreEqual(cmdOne.NoteText, cmdTwo.NoteText);
            Assert.AreEqual(cmdOne.X, cmdTwo.X, 0.000001);
            Assert.AreEqual(cmdOne.Y, cmdTwo.Y, 0.000001);
            Assert.AreEqual(cmdOne.DefaultPosition, cmdTwo.DefaultPosition);
        }

        [Test, RequiresSTA]
        public void TestSelectModelCommand()
        {
            Guid modelGuid = Guid.NewGuid();
            ModifierKeys modifiers = ((randomizer.Next(2) == 0) ?
                ModifierKeys.Control : ModifierKeys.Alt);

            var cmdOne = new DynCmd.SelectModelCommand(modelGuid, modifiers);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.ModelGuid, cmdTwo.ModelGuid);
            Assert.AreEqual(cmdOne.Modifiers, cmdTwo.Modifiers);
        }

        [Test, RequiresSTA]
        public void TestSelectInRegionCommand()
        {
            Rect region = new Rect(
                randomizer.NextDouble() * 100,
                randomizer.NextDouble() * 100,
                randomizer.NextDouble() * 100,
                randomizer.NextDouble() * 100);

            bool isCrossSelection = randomizer.Next(2) == 0;

            var cmdOne = new DynCmd.SelectInRegionCommand(region, isCrossSelection);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.Region.X, cmdTwo.Region.X, 0.000001);
            Assert.AreEqual(cmdOne.Region.Y, cmdTwo.Region.Y, 0.000001);
            Assert.AreEqual(cmdOne.Region.Width, cmdTwo.Region.Width, 0.000001);
            Assert.AreEqual(cmdOne.Region.Height, cmdTwo.Region.Height, 0.000001);
            Assert.AreEqual(cmdOne.IsCrossSelection, cmdTwo.IsCrossSelection);
        }

        [Test, RequiresSTA]
        public void TestDragSelectionCommand()
        {
            Point point = new Point(
                randomizer.NextDouble() * 100,
                randomizer.NextDouble() * 100);

            var operation = ((randomizer.Next(2) == 0) ?
                DynCmd.DragSelectionCommand.Operation.BeginDrag :
                DynCmd.DragSelectionCommand.Operation.EndDrag);

            var cmdOne = new DynCmd.DragSelectionCommand(point, operation);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.MouseCursor.X, cmdTwo.MouseCursor.X, 0.000001);
            Assert.AreEqual(cmdOne.MouseCursor.Y, cmdTwo.MouseCursor.Y, 0.000001);
            Assert.AreEqual(cmdOne.DragOperation, cmdTwo.DragOperation);
        }

        [Test, RequiresSTA]
        public void TestMakeConnectionCommand()
        {
            Guid nodeId = Guid.NewGuid();
            int portIndex = randomizer.Next();
            var portType = ((PortType)randomizer.Next(2));
            var mode = ((DynCmd.MakeConnectionCommand.Mode)randomizer.Next(3));

            var cmdOne = new DynCmd.MakeConnectionCommand(
                nodeId, portIndex, portType, mode);

            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.NodeId, cmdTwo.NodeId);
            Assert.AreEqual(cmdOne.PortIndex, cmdTwo.PortIndex);
            Assert.AreEqual(cmdOne.Type, cmdTwo.Type);
            Assert.AreEqual(cmdOne.ConnectionMode, cmdTwo.ConnectionMode);
        }

        [Test, RequiresSTA]
        public void TestDeleteModelCommand()
        {
            Guid modelGuid = Guid.NewGuid();
            var cmdOne = new DynCmd.DeleteModelCommand(modelGuid);
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.ModelGuid, cmdTwo.ModelGuid);
        }

        [Test, RequiresSTA]
        public void TestUndoRedoCommand()
        {
            var operation = ((DynCmd.UndoRedoCommand.Operation)randomizer.Next(2));
            var cmdOne = new DynCmd.UndoRedoCommand(operation);
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.CmdOperation, cmdTwo.CmdOperation);
        }

        [Test, RequiresSTA]
        public void TestUpdateModelValueCommand()
        {
            Guid modelGuid = Guid.NewGuid();
            string name = randomizer.Next().ToString();
            string value = randomizer.Next().ToString();

            var cmdOne = new DynCmd.UpdateModelValueCommand(modelGuid, name, value);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.ModelGuid, cmdTwo.ModelGuid);
            Assert.AreEqual(cmdOne.Name, cmdTwo.Name);
            Assert.AreEqual(cmdOne.Value, cmdTwo.Value);
        }

        [Test, RequiresSTA]
        public void TestCreateNodes()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            Assert.AreEqual(5, workspace.Nodes.Count);
        }

        [Test, RequiresSTA]
        public void TestCreateConnectors()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void TestDeleteCommands()
        {
            RunCommandsFromFile("CreateAndDeleteNodes.xml");
            Assert.AreEqual(4, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);

            // This dictionary maps each of the node GUIDs, to a Boolean 
            // flag indicating that if the node exists or deleted.
            Dictionary<string, bool> nodeExistenceMap = new Dictionary<string, bool>()
            {
                { "ba59fa31-919d-4e67-b7c6-b58589a7093f", true },
                { "42058bba-c2fd-4e49-8d76-44c45d0dc597", false },
                { "5c92e961-8095-49bb-828d-1f3c14f9a005", true },
                { "d5ad0ff6-9314-4e22-947f-7ba967ad4758", false },
                { "4d2b71b4-d2c1-4695-afcf-6f7ec05c71f5", true },
                { "a71328b2-dee7-45d6-8070-44ecebc358d9", true },
            };

            VerifyModelExistence(nodeExistenceMap);
        }

        [Test, RequiresSTA]
        public void TestUndoRedoNodesAndConnections()
        {
            RunCommandsFromFile("UndoRedoNodesAndConnections.xml");
            Assert.AreEqual(2, workspace.Connectors.Count);

            // This dictionary maps each of the node GUIDs, to a Boolean 
            // flag indicating that if the node exists or deleted.
            Dictionary<string, bool> nodeExistenceMap = new Dictionary<string, bool>()
            {
                { "fec0ae4f-f3b7-4b33-b728-c75e5415d73c", true },
                { "168298c7-f003-48f8-a346-0061086f8e3a", true },
                { "69ee3a47-0a9a-4746-ace3-6643d508f235", true },
            };

            VerifyModelExistence(nodeExistenceMap);
        }

        [Test, RequiresSTA]
        public void TestUpdateNodeContents()
        {
            RunCommandsFromFile("UpdateNodeContents.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(5, workspace.Nodes.Count);

            var number = GetNode("2ba65a2e-c3dd-4d27-9d18-9bf123835fb8") as DoubleInput;
            var slider = GetNode("2279f845-4ba9-4300-a6c3-a566cd8b4a32") as DoubleSliderInput;
            var strIn = GetNode("d33abcb6-50fd-4d18-ac89-87adb2d28053") as StringInput;
            var formula = GetNode("540fffbb-4f5b-4496-9231-eba5b04e388c") as Formula;
            var sublist = GetNode("0a60f132-25a0-4b7c-85f2-3c31f39ef9da") as Sublists;

            Assert.IsNotNull(number);
            Assert.IsNotNull(slider);
            Assert.IsNotNull(strIn);
            Assert.IsNotNull(formula);
            Assert.IsNotNull(sublist);

            Assert.AreEqual("12.34", number.Value);
            Assert.AreEqual(23.45, slider.Min, 0.000001);
            Assert.AreEqual(34.56, slider.Value, 0.000001);
            Assert.AreEqual(45.67, slider.Max, 0.000001);
            Assert.AreEqual("Test String Input", strIn.Value);
            Assert.AreEqual("d", sublist.Value);

            Assert.AreEqual("a+b+c", formula.FormulaString);
            Assert.AreEqual(3, formula.InPorts.Count);
            Assert.AreEqual(1, formula.OutPorts.Count);
        }

        [Test, RequiresSTA]
        public void TestUpdateNodeCaptions()
        {
            RunCommandsFromFile("UpdateNodeCaptions.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(1, workspace.Notes.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var number = GetNode("0b171995-528b-480a-b203-9cee49fcec9d") as DoubleInput;
            var strIn = GetNode("d17de86f-0665-4e22-abd4-d16360ee17d7") as StringInput;
            var note = GetNode("6aed237b-beb6-4a24-8774-9b7e29615be1") as NoteModel;

            Assert.IsNotNull(number);
            Assert.IsNotNull(strIn);
            Assert.IsNotNull(note);

            Assert.AreEqual("Caption 1", number.NickName);
            Assert.AreEqual("Caption 2", strIn.NickName);
            Assert.AreEqual("Caption 3", note.Text);
        }

        [Test, RequiresSTA]
        public void TestVerifyRuntimeValues()
        {
            RunCommandsFromFile("VerifyRuntimeValues.xml", true);
            Assert.AreEqual(2, workspace.Connectors.Count);
            Assert.AreEqual(3, workspace.Nodes.Count);

            var number1 = GetNode("76b951e9-a815-4fb9-bec1-fbd1178fa113") as DoubleInput;
            var number2 = GetNode("1a3efb71-52df-46e8-95ab-a130e9a885ce") as DoubleInput;
            var addition = GetNode("9182323d-a4fd-40eb-905b-8ec415d17926") as Addition;

            Assert.AreEqual(12.34, (number1.OldValue as FScheme.Value.Number).Item);
            Assert.AreEqual(56.78, (number2.OldValue as FScheme.Value.Number).Item);
            Assert.AreEqual(69.12, (addition.OldValue as FScheme.Value.Number).Item);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_491()
        {
            RunCommandsFromFile("Defect-MAGN-491.xml");
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(connectors);
            Assert.AreEqual(2, connectors.Count);

            // Get to the only two connectors in the session.
            ConnectorViewModel firstConnector = connectors[0];
            ConnectorViewModel secondConnector = connectors[1];

            // Find out the corresponding ports they connect to.
            Point firstPoint = firstConnector.ConnectorModel.End.Center;
            Point secondPoint = secondConnector.ConnectorModel.End.Center;

            Assert.AreEqual(firstPoint.X, firstConnector.CurvePoint3.X);
            Assert.AreEqual(firstPoint.Y, firstConnector.CurvePoint3.Y);
            Assert.AreEqual(secondPoint.X, secondConnector.CurvePoint3.X);
            Assert.AreEqual(secondPoint.Y, secondConnector.CurvePoint3.Y);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_225()
        {
            RunCommandsFromFile("TestConnectionReplacementUndo.xml");
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(3, nodes.Count);
        }


        [Test, RequiresSTA]
        public void TestBasicCodeBlockNodePortCreation()
        {
            RunCommandsFromFile("TestBasicPortCreation.xml");
            
            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(2, nodes.Count);

            //Check the CBN
            var cbn = GetNode("107e30e9-e97c-402c-b206-d27162d1fafd") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.ERROR, cbn.State); 
            Assert.AreEqual(4, cbn.OutPorts.Count);
            Assert.AreEqual(2, cbn.InPorts.Count);

            //CBN OutPut Ports 
            //    > ToolTipContent stores name of variable
            //    > Margina thickness is for height.(is a multiple of 20, except for the first)
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("b", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[1].MarginThickness.Top);

            Assert.AreEqual("c", cbn.OutPorts[2].ToolTipContent);
            Assert.AreEqual(60, cbn.OutPorts[2].MarginThickness.Top);

            Assert.AreEqual("d", cbn.OutPorts[3].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[3].MarginThickness.Top);

            //CBN Input Ports
            //   >PortName stores name of variable
            Assert.AreEqual("x", cbn.InPorts[0].PortName);
            Assert.AreEqual("y", cbn.InPorts[1].PortName);

            //Check the connections
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(connectors);
            Assert.AreEqual(2, connectors.Count);
        }

        private ModelBase GetNode(string guid)
        {
            Guid id = Guid.Parse(guid);
            return workspace.GetModelInternal(id);
        }

        private void VerifyModelExistence(Dictionary<string, bool> modelExistenceMap)
        {
            var nodes = workspace.Nodes;
            foreach (var pair in modelExistenceMap)
            {
                Guid guid = Guid.Parse(pair.Key);
                var node = nodes.FirstOrDefault((x) => (x.GUID == guid));
                bool nodeExists = (null != node);
                Assert.AreEqual(nodeExists, pair.Value);
            }
        }

        private void RunCommandsFromFile(string commandFileName, bool autoRun = false)
        {
            string commandFilePath = DynamoTestUI.GetTestDirectory();
            commandFilePath = Path.Combine(commandFilePath, @"core\recorded\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            // Create the controller to run alongside the view.
            controller = DynamoController.MakeSandbox(commandFilePath);
            controller.DynamoViewModel.DynamicRunEnabled = autoRun;

            // Create the view.
            var dynamoView = new DynamoView();
            dynamoView.DataContext = controller.DynamoViewModel;
            controller.UIDispatcher = dynamoView.Dispatcher;
            dynamoView.ShowDialog();

            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.DynamoModel);
            Assert.IsNotNull(controller.DynamoModel.CurrentWorkspace);
            workspace = controller.DynamoModel.CurrentWorkspace;
            workspaceViewModel = controller.DynamoViewModel.CurrentSpaceViewModel;
        }

        private CmdType DuplicateAndCompare<CmdType>(CmdType command)
            where CmdType : DynCmd.RecordableCommand
        {
            Assert.IsNotNull(command); // Ensure we have an input command.

            // Serialize the command into an XmlElement.
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement element = command.Serialize(xmlDocument);
            Assert.IsNotNull(element);

            // Deserialized the XmlElement into a new instance of the command.
            var duplicate = DynCmd.RecordableCommand.Deserialize(element);
            Assert.IsNotNull(duplicate);
            Assert.IsTrue(duplicate is CmdType);
            return duplicate as CmdType;
        }
    }
}
