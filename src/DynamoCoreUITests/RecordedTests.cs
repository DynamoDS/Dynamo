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
using Dynamo.Tests;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using Dynamo.Tests;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class RecordedTests
    {
        #region Generic Set-up Routines and Data Members

        private System.Random randomizer = null;

        // For access within test cases.
        protected WorkspaceModel workspace = null;
        protected WorkspaceViewModel workspaceViewModel = null;
        protected DynamoController controller = null;

        [SetUp]
        public void Start()
        {
            // Fixed seed randomizer for predictability.
            randomizer = new System.Random(123456);
        }

        [TearDown]
        public void Exit()
        {
            try
            {
                this.controller.ShutDown();
            }
            catch (Exception e)
            {
            }
            this.controller = null;
        }

        private string GetVarName(string guid)
        {
            var model = controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.VariableToPreview;
        }

        private RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            mirror = controller.EngineController.GetMirror(varName);

            Assert.IsNotNull(mirror);
            return mirror;
        }

        public void AssertValue(string varname, object value)
        {
            var mirror = GetRuntimeMirror(varname);

            Console.WriteLine(varname + " = " + mirror.GetStringData());
            StackValue svValue = mirror.GetData().GetStackValue();

            if (value == null)
            {
                Assert.IsTrue(StackUtils.IsNull(svValue));
            }
            else if (value is double)
            {
                Assert.AreEqual(svValue.opdata_d, Convert.ToDouble(value));
            }
            else if (value is int)
            {
                Assert.AreEqual(svValue.opdata, Convert.ToInt64(value));
            }
            else if (value is IEnumerable<int>)
            {
                var values = (value as IEnumerable<int>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(Int64)));
            }
            else if (value is IEnumerable<double>)
            {
                var values = (value as IEnumerable<double>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(double)));
            }
        }

        private void AssertPreviewValue(string guid, object value)
        {
            string previewVariable = GetVarName(guid);
            AssertValue(previewVariable, value);
        }

        private void AssertIsPointer(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);

            StackValue svValue = mirror.GetData().GetStackValue();
            Assert.IsTrue(StackUtils.IsValidPointer(svValue));
        }

        #endregion

        #region Recorded Test Cases for Command Framework

        [Test, RequiresSTA]
        public void _SnowPeashooter()
        {
            RunCommandsFromFile("SnowPeashooter.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            var number = GetNode("045decd1-7454-4b85-b92e-d59d35f31ab2") as DoubleInput;
            Assert.AreEqual("12.34", number.Value);
        }

        [Test, RequiresSTA]
        public void TestRunCancelCommand()
        {
            bool showErrors = randomizer.Next(2) == 0;
            bool cancelRun = randomizer.Next(2) == 0;

            var cmdOne = new DynCmd.RunCancelCommand(showErrors, cancelRun);
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.ShowErrors, cmdTwo.ShowErrors);
            Assert.AreEqual(cmdOne.CancelRun, cmdTwo.CancelRun);
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

        #endregion

        #region Private Helper Methods

        protected ModelBase GetNode(string guid)
        {
            Guid id = Guid.Parse(guid);
            return workspace.GetModelInternal(id);
        }

        protected void VerifyModelExistence(Dictionary<string, bool> modelExistenceMap)
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

        protected void RunCommandsFromFile(string commandFileName, bool autoRun = false)
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

        #endregion
    }

#if !USE_DSENGINE

    class RecordedTestsFScheme : RecordedTests
    {
        [Test, RequiresSTA]
        public void Defect_MAGN_159()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-159

            RunCommandsFromFile("Defect_MAGN_159.xml", true);

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            var number1 = GetNode("045decd1-7454-4b85-b92e-d59d35f31ab2") as DoubleInput;
            Assert.AreEqual(8, (number1.OldValue as FScheme.Value.Number).Item);
        }

        [Ignore, RequiresSTA]
        public void Defect_MAGN_160()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-160

            // List node cannot be created  ( current limitation for button click)
            RunCommandsFromFile("Defect_MAGN_160.xml");

            //Assert.AreEqual(1, workspace.Nodes.Count);
            //Assert.AreEqual(0, workspace.Connectors.Count);

            //var number1 = GetNode("045decd1-7454-4b85-b92e-d59d35f31ab2") as DoubleInput;
            //Assert.AreEqual(8, (number1.OldValue as FScheme.Value.Number).Item);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_164()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-164

            RunCommandsFromFile("Defect_MAGN_164.xml", true);

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            var number1 = GetNode("2e1e5f33-52fc-4cc9-9d4a-33e46ec64a53") as DoubleInput;
            Assert.AreEqual(30, (number1.OldValue as FScheme.Value.Number).Item);

            var string1 = GetNode("a4ba7320-3cb8-4524-bc8c-8688d7abc599") as StringInput;
            Assert.AreEqual("Dynamo", (string1.OldValue as FScheme.Value.String).Item);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_190()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-190
            RunCommandsFromFile("Defect_MAGN_190.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_225()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-225

            // TODO: Rename this XML to match the test case name.
            RunCommandsFromFile("TestConnectionReplacementUndo.xml");
            var nodes = workspaceViewModel.Nodes;

            Assert.NotNull(nodes);
            Assert.AreEqual(3, nodes.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_397()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-397
            RunCommandsFromFile("Defect_MAGN_397.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_429()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-429
            RunCommandsFromFile("Defect_MAGN_429.xml");

            Assert.AreEqual(0, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_478()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-478
            RunCommandsFromFile("Defect_MAGN_478.xml");

            Assert.AreEqual(1, workspace.Notes.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_491()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-491

            // TODO: Rename this XML to match the test case name.
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
        public void Defect_MAGN_520()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520
            RunCommandsFromFile("Defect_MAGN_520.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_520_WithCrossSelection()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520
            RunCommandsFromFile("Defect_MAGN_520_WithCrossSelection.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_57()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-57
            RunCommandsFromFile("Defect_MAGN_57.xml");

            Assert.AreEqual(7, workspace.Nodes.Count);
            Assert.AreEqual(5, workspace.Connectors.Count);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_581()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-581
            RunCommandsFromFile("Defect_MAGN_581.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void ShiftSelectAllNode()
        {
            RunCommandsFromFile("ShiftSelectAllNode.xml");

            Assert.AreEqual(4, workspace.Nodes.Count);
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void TestCreateConnectors()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void TestCreateNodesAndRunExpression()
        {
            RunCommandsFromFile("CreateNodesAndRunExpression.xml");
            var number1 = GetNode("e37873fb-ef3f-4864-b7e5-9417e0ad014c") as DoubleInput;
            var number2 = GetNode("977ce97c-22f5-4155-ae22-d0c3a6f82f19") as DoubleInput;
            var addition = GetNode("cf8c52b1-fbee-4674-ba73-6ee0d09463f2") as Addition;

            Assert.IsNotNull(number1);
            Assert.IsNotNull(number2);
            Assert.IsNotNull(addition);

            Assert.AreEqual(4, ((number1.OldValue as FScheme.Value.Number).Item), 0.000001);
            Assert.AreEqual(2, ((number2.OldValue as FScheme.Value.Number).Item), 0.000001);
            Assert.AreEqual(6, ((addition.OldValue as FScheme.Value.Number).Item), 0.000001);
        }

        [Test, RequiresSTA]
        public void TestCreateNodes()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            Assert.AreEqual(5, workspace.Nodes.Count);
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
        public void TestModifyPythonNodes()
        {
            RunCommandsFromFile("ModifyPythonNodes.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var python = GetNode("6f580b72-6aeb-4af2-b28b-a2e5b634721b") as Python;
            var pvarin = GetNode("f0fc1dea-3874-40a0-a532-90c0ee10f437") as PythonVarIn;

            Assert.AreEqual("# Modification 3", python.Script);
            Assert.AreEqual("# Modification 4", pvarin.Script);
        }

        [Test, RequiresSTA]
        public void TestModifyPythonNodesUndo()
        {
            RunCommandsFromFile("ModifyPythonNodesUndo.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var python = GetNode("6f580b72-6aeb-4af2-b28b-a2e5b634721b") as Python;
            var pvarin = GetNode("f0fc1dea-3874-40a0-a532-90c0ee10f437") as PythonVarIn;

            Assert.AreEqual("# Modification 1", python.Script);
            Assert.AreEqual("# Modification 2", pvarin.Script);
        }

        [Test, RequiresSTA]
        public void TestModifyPythonNodesUndoRedo()
        {
            RunCommandsFromFile("ModifyPythonNodesUndoRedo.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var python = GetNode("6f580b72-6aeb-4af2-b28b-a2e5b634721b") as Python;
            var pvarin = GetNode("f0fc1dea-3874-40a0-a532-90c0ee10f437") as PythonVarIn;

            Assert.AreEqual("# Modification 3", python.Script);
            Assert.AreEqual("# Modification 4", pvarin.Script);
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

    }

#else

    class RecordedTestsDSEngine : RecordedTests
    {
        #region Basic CodeBlockNode Test Cases

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
            Assert.AreNotEqual(ElementState.Error, cbn.State);
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

        /// <summary>
        /// Creates a Code Block Node with a single line comment and multi line comment 
        /// checks if the ports are created properly and at the correct height
        /// </summary>
        [Test, RequiresSTA]
        public void TestCommentsInCodeBlockNode()
        {
            RunCommandsFromFile("TestCommentsInCodeBlockNode.xml");

            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);

            //Check the CBN
            var cbn = GetNode("ebcaa0d3-3f8a-48a7-b5c0-986e383357de") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);

            Assert.AreEqual("c", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(100, cbn.OutPorts[1].MarginThickness.Top);
        }

        /// <summary>
        /// Create a code block node with some ports connected and others unconnected. Change all variable names
        /// and ensure that connectors remain to the port index.
        /// </summary>
        [Test, RequiresSTA]
        public void TestCodeBlockNodeConnectionOnCodeChange()
        {
            RunCommandsFromFile("TestCodeBlockNodeConnectionSwitching.xml");

            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(2, nodes.Count);

            //Check the CBN
            var cbn = GetNode("37fade4a-e7ad-43ae-8b6f-27dacb17c1c5") as CodeBlockNodeModel;
            Assert.AreEqual(4, cbn.OutPorts.Count);

            //Check starting point of connector
            Assert.AreEqual(0, cbn.OutPorts[0].Connectors.Count);
            Assert.AreEqual(1, cbn.OutPorts[1].Connectors.Count);
            Assert.AreEqual(0, cbn.OutPorts[2].Connectors.Count);
            Assert.AreEqual(1, cbn.OutPorts[3].Connectors.Count);

            //CheckEnding point
            Assert.AreEqual(1, cbn.OutPorts[1].Connectors[0].End.Index);
            Assert.AreEqual(0, cbn.OutPorts[3].Connectors[0].End.Index);
        }

        /// <summary>
        /// Creates 3 number nodes and an add (+) nodes. Connects 2 of the number
        /// nodes to the + node. Then converts all the nodes to Code.
        /// </summary>
        [Test, RequiresSTA]
        public void TestConvertAllNodesToCode()
        {
            RunCommandsFromFile("TestConvertAllNodesToCode.xml");

            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);

            //Check the connectors
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(connectors);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN
            var cbn = GetNode("8950950f-78f3-4d81-8181-c574ad84bb1d") as CodeBlockNodeModel;
            Assert.AreEqual(4, cbn.OutPorts.Count);
            Assert.True(cbn.Code.Contains("+"));
            Assert.True(cbn.Code.Contains("14"));
            Assert.True(cbn.Code.Contains("190"));
            Assert.True(cbn.Code.Contains("69"));
        }

        /// <summary>
        /// Converts a set of nodes to code. Then does Undo and checks that the original set of
        /// nodes are formed again.
        /// </summary>
        [Test, RequiresSTA]
        public void TestConvertAllNodesToCodeUndo()
        {
            RunCommandsFromFile("TestConvertAllNodesToCodeUndo.xml");

            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(4, nodes.Count);

            //Check the connectors
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(connectors);
            Assert.AreEqual(2, connectors.Count);

            //Check that there is no CBN
            var cbn = GetNode("37fade4a-e7ad-43ae-8b6f-27dacb17c1c5") as CodeBlockNodeModel;
            Assert.AreEqual(null, cbn);

            var addNode = workspaceViewModel._model.Nodes.Where(x => x is DSFunction).First() as DSFunction;
            Assert.NotNull(addNode);

            var numberList = workspaceViewModel._model.Nodes.Where(x => x is DoubleInput).ToList<NodeModel>();
            Assert.AreEqual(3, numberList.Count);
        }

        /// <summary>
        /// Ensures that redo works for NodeToCode by converting a set of nodes to
        /// code and then undoing and redoing it again.
        /// </summary>
        [Test, RequiresSTA]
        public void TestConvertAllNodesToCodeUndoRedo()
        {
            RunCommandsFromFile("TestConvertAllNodesToCodeUndoRedo.xml");

            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);

            //Check the connectors
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(connectors);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN
            var cbn = GetNode("8950950f-78f3-4d81-8181-c574ad84bb1d") as CodeBlockNodeModel;
            Assert.AreEqual(4, cbn.OutPorts.Count);
            Assert.True(cbn.Code.Contains("+"));
            Assert.True(cbn.Code.Contains("14"));
            Assert.True(cbn.Code.Contains("190"));
            Assert.True(cbn.Code.Contains("69"));
        }
            
        [Ignore, RequiresSTA]
        public void TestDeleteCommands_DS()
        {
            RunCommandsFromFile("TestDeleteCommands_DS.xml");
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
        public void TestUndoRedoNodesAndConnections_DS()
        {
            RunCommandsFromFile("TestUndoRedoNodesAndConnection_DS.xml");
            Assert.AreEqual(2, workspace.Connectors.Count);

            // This dictionary maps each of the node GUIDs, to a Boolean 
            // flag indicating that if the node exists or deleted.
            Dictionary<string, bool> nodeExistenceMap = new Dictionary<string, bool>()
            {
                { "2605ed9d-1cce-41a2-8b36-dcd02d1396a6", true },
                { "9beac565-3238-4396-8c78-9d9645ec5185", true },
                { "a40978be-1877-478d-8935-fa6b01334055", true },
            };

            VerifyModelExistence(nodeExistenceMap);
        }

        [Test, RequiresSTA]
        public void TestUpdateNodeCaptions_DS()
        {
            RunCommandsFromFile("TestUpdateNodeCaptions_DS.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var cbn = GetNode("5cf9dff2-4a3e-428a-a98a-60d0de0d323e") as CodeBlockNodeModel;

            Assert.IsNotNull(cbn);

            Assert.AreEqual("CBN", cbn.NickName);
        }

        #endregion

        #region Defect Verifications Test Cases

        [Test, RequiresSTA]
        public void Defect_MAGN_159()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-159

            RunCommandsFromFile("Defect_MAGN_159.xml", true);

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            var number1 = GetNode("045decd1-7454-4b85-b92e-d59d35f31ab2") as DoubleInput;
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_164_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-904
            RunCommandsFromFile("Defect_MAGN_164_DS.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("60158259-4d9a-4bc0-b80c-aea9a90c2b57") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_190_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-904
            RunCommandsFromFile("Defect_MAGN_190_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("55cf8f57-5eff-4e0b-b547-3d6cb26bc236") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_225_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-225

            RunCommandsFromFile("Defect_MAGN_225_DS.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_397_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-397
            RunCommandsFromFile("Defect_MAGN_397_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_411()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-411
            RunCommandsFromFile("Defect_MAGN_411.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);

            var cbn = GetNode("fc209d2f-1724-4485-bde4-92670802aaa3") as CodeBlockNodeModel;
            Assert.NotNull(cbn);

            Assert.AreEqual(2, cbn.InPortData.Count);
            Assert.AreEqual("a", cbn.InPortData[0].ToolTipString);
            Assert.AreEqual("b", cbn.InPortData[1].ToolTipString);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_429_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-429
            RunCommandsFromFile("Defect_MAGN_429_DS.xml");

            Assert.AreEqual(0, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_478_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-478

            // Same XML can be used for this test case as well.
            RunCommandsFromFile("Defect_MAGN_478.xml");

            Assert.AreEqual(1, workspace.Notes.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_491_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-491

            // TODO: Rename this XML to match the test case name.
            RunCommandsFromFile("Defect_MAGN_491_DS.xml");
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(connectors);
            Assert.AreEqual(2, connectors.Count);
            Assert.AreEqual(3, workspace.Nodes.Count);

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
        public void Defect_MAGN_520_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520
            RunCommandsFromFile("Defect_MAGN_520_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_520_WithCrossSelection_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520

            // Same XML can be used for this test case as well.
            RunCommandsFromFile("Defect_MAGN_520_WithCrossSelection.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_581_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-581
            RunCommandsFromFile("Defect_MAGN_581_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_590()
        {
            RunCommandsFromFile("Defect-MAGN-590.xml");

            //Check the nodes
            var nodes = workspaceViewModel.Nodes;
            Assert.NotNull(nodes);
            Assert.AreEqual(2, nodes.Count);

            //Check the CBN
            var cbn = GetNode("8630afc1-3d59-4e76-9fca-faa12e6973ea") as CodeBlockNodeModel;
            var connector = cbn.OutPorts[1].Connectors[0] as ConnectorModel;
            Assert.AreEqual(2, connector.End.Index);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_775()
        {
            // The third undo operation should not crash.
            RunCommandsFromFile("Defect_MAGN_775.xml");
            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_585()
        {
            // Details steps are here : http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-585

            RunCommandsFromFile("Defect_MAGN_585.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(2, nodes.Count);
            Assert.AreEqual(2, connectors.Count);

            //Check the CBN
            var cbn = GetNode("5dd0c52b-aa33-4db0-bbe6-e653c1b2a73a") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(5, cbn.OutPorts.Count);
            Assert.AreEqual(1, cbn.InPorts.Count);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_605()
        {
            // Details steps are here : http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-605

            RunCommandsFromFile("Defect_MAGN_605.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN
            var cbn = GetNode("a344e085-a6fa-4d43-ac27-692fb102ba6d") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("b", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(0, cbn.OutPorts[1].MarginThickness.Top);

            Assert.AreEqual("c", cbn.OutPorts[2].ToolTipContent);
            Assert.AreEqual(0, cbn.OutPorts[2].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_624()
        {
            // Details steps are here : http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-624

            RunCommandsFromFile("Defect_MAGN_624.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("8bc43138-d655-40f6-973e-614f1695874c") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(24, cbn.OutPorts[0].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_624_1()
        {
            // Further testing of this defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-624
            // a={1,2,3};
            // a[0] = 3; // first create CBN with first two lines and then add two more. the below one.
            // b = 1;
            // a = 0;

            RunCommandsFromFile("Defect_MAGN_624_1.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(2, nodes.Count);
            Assert.AreEqual(2, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("c9929987-69c8-42bd-9cda-04ef90d029cb") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("b", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(64, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("a", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[1].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_590_1()
        {
            // Further testing of this defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-590

            RunCommandsFromFile("Defect_MAGN_590.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(2, nodes.Count);
            Assert.AreEqual(2, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("88295180-7478-4c70-af15-cdac34835abf") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("c", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[1].MarginThickness.Top);

            var connector = cbn.OutPorts[1].Connectors[0] as ConnectorModel;
            Assert.AreEqual(2, connector.End.Index);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_589_1()
        {
            // Further testing of this defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-589

            RunCommandsFromFile("Defect_MAGN_589_1.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("08cdbdea-a025-4cc6-a449-66896cdfa319") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(2, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("Statement Output", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[1].MarginThickness.Top);

            Assert.AreEqual("Statement Output", cbn.OutPorts[2].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[2].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_589_2()
        {
            // Further testing of this defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-589

            RunCommandsFromFile("Defect_MAGN_589_2.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("9b225999-1803-4627-b319-d32ccbea33ef") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("b", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[1].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_589_3()
        {
            // Further testing of this defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-589

            RunCommandsFromFile("Defect_MAGN_589_3.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("623aa74b-bf03-4169-98d9-bee76feb1f3b") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(5, cbn.OutPorts.Count);
            Assert.AreEqual(2, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("Statement Output", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[1].MarginThickness.Top);

            Assert.AreEqual("d", cbn.OutPorts[2].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[2].MarginThickness.Top);

            Assert.AreEqual("Statement Output", cbn.OutPorts[3].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[3].MarginThickness.Top);

            Assert.AreEqual("h", cbn.OutPorts[4].ToolTipContent);
            Assert.AreEqual(20, cbn.OutPorts[4].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_828()
        {
            // Further testing of this defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-828

            RunCommandsFromFile("Defect_MAGN_828.xml");

            //Check the nodes and connectors count
            var nodes = workspaceViewModel.Nodes;
            var connectors = workspaceViewModel.Connectors;
            Assert.NotNull(nodes);
            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(0, connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("32542274-9e86-4ac6-8288-3f3ac8d6e906") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(1, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("Statement Output", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_613()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-613
            RunCommandsFromFile("Defect_MAGN_613.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("3c7c3458-70be-4588-b162-b1099cf30ebc") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(64, cbn.OutPorts[0].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_904()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-904
            RunCommandsFromFile("Defect_MAGN_904.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("3a379c45-d128-467b-a530-2b741d330dc4") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(2, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("t_4", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("t_1", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(0, cbn.OutPorts[1].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_830()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-830
            RunCommandsFromFile("Defect_MAGN_830.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("4b2b7966-a24c-44fe-b2f0-9aed54b72319") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            //Assert.AreEqual("t_2", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            //Assert.AreEqual("t_1", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(0, cbn.OutPorts[1].MarginThickness.Top);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_803()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-803
            RunCommandsFromFile("Defect_MAGN_803.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("09eb3645-f5e9-4186-8543-2195e7740eb2") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

        }

        /// <summary>
        /// Temporary workaround
        /// Test case on creating simple operations and compare with NodeToCode value and Undo
        /// Eventually the evaluation will be done at record playback side thus remove the need
        /// of having multiple files.
        /// </summary>
        [Test, RequiresSTA]
        public void TestCBNWithNodeToCode()
        {
            RunCommandsFromFile("TestCBNOperationWithoutNodeToCode.xml");
            AssertValue("c", 8); // Run playback is recorded in command file

            // Reset current test case
            Exit();
            Start();

            RunCommandsFromFile("TestCBNOperationWithNodeToCode.xml");
            AssertValue("c", 8); // Run playback is recorded in command file

            // Reset current test case
            Exit();
            Start();

            RunCommandsFromFile("TestCBNOperationWithNodeToCodeUndo.xml");
            AssertValue("c", 8); // Run playback is recorded in command file
        }

        #endregion

        #region Private Helper Methods

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
            controller.Testing = true;
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

        #endregion
    }

#endif
}
