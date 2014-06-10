﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using DSIronPythonNode;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    public delegate void CommandCallback(string commandTag);

    [TestFixture]
    public class RecordedTests : DSEvaluationUnitTest
    {
        #region Generic Set-up Routines and Data Members

        private System.Random randomizer = null;
        private IEnumerable<string> customNodesToBeLoaded = null;
        private CommandCallback commandCallback = null;

        // For access within test cases.
        protected WorkspaceModel workspace = null;
        protected WorkspaceViewModel workspaceViewModel = null;

        public override void Init()
        {
            // We do not call "base.Init()" here because we want to be able 
            // to create our own copy of Controller here with command file path.
        }

        [SetUp]
        public void Start()
        {
            // Fixed seed randomizer for predictability.
            randomizer = new System.Random(123456);
            SetupDirectories();
        }

        [TearDown]
        protected void Exit()
        {
            commandCallback = null;
            if (this.Controller != null)
            {
                this.Controller.ShutDown(true);
                this.Controller = null;
            }

            GC.Collect();
        }

        #endregion

        #region Recorded Test Cases for Command Framework

        [Test, RequiresSTA]
        public void _SnowPeashooter()
        {
            //RunCommandsFromFile("SnowPeashooter.xml");

            //Assert.AreEqual(1, workspace.Nodes.Count);
            //Assert.AreEqual(0, workspace.Connectors.Count);

            //var number = GetNode("045decd1-7454-4b85-b92e-d59d35f31ab2") as DoubleInput;
            //Assert.AreEqual("12.34", number.Value);

            Assert.Inconclusive("Porting : DoubleInput");
        }

        [Test, RequiresSTA]
        public void TestPausePlaybackCommand()
        {
            int pauseDurationInMs = randomizer.Next(2000);

            var cmdOne = new DynamoViewModel.PausePlaybackCommand(pauseDurationInMs);
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.PauseDurationInMs, cmdTwo.PauseDurationInMs);
        }

        [Test, RequiresSTA]
        public void TestRunCancelCommand()
        {
            bool showErrors = randomizer.Next(2) == 0;
            bool cancelRun = randomizer.Next(2) == 0;

            var cmdOne = new DynamoViewModel.RunCancelCommand(showErrors, cancelRun);
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

            var cmdOne = new DynamoViewModel.CreateNodeCommand(
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

            var cmdOne = new DynamoViewModel.CreateNoteCommand(nodeId, text, x, y, defaultPos);
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

            var cmdOne = new DynamoViewModel.SelectModelCommand(modelGuid, modifiers);
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

            var cmdOne = new DynamoViewModel.SelectInRegionCommand(region, isCrossSelection);
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
                DynamoViewModel.DragSelectionCommand.Operation.BeginDrag :
                DynamoViewModel.DragSelectionCommand.Operation.EndDrag);

            var cmdOne = new DynamoViewModel.DragSelectionCommand(point, operation);
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
            var mode = ((DynamoViewModel.MakeConnectionCommand.Mode)randomizer.Next(3));

            var cmdOne = new DynamoViewModel.MakeConnectionCommand(
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
            var cmdOne = new DynamoViewModel.DeleteModelCommand(modelGuid);
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.ModelGuid, cmdTwo.ModelGuid);
        }

        [Test, RequiresSTA]
        public void TestUndoRedoCommand()
        {
            var operation = ((DynamoViewModel.UndoRedoCommand.Operation)randomizer.Next(2));
            var cmdOne = new DynamoViewModel.UndoRedoCommand(operation);
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.CmdOperation, cmdTwo.CmdOperation);
        }

        [Test, RequiresSTA]
        public void TestUpdateModelValueCommand()
        {
            Guid modelGuid = Guid.NewGuid();
            string name = randomizer.Next().ToString();
            string value = randomizer.Next().ToString();

            var cmdOne = new DynamoViewModel.UpdateModelValueCommand(modelGuid, name, value);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.ModelGuid, cmdTwo.ModelGuid);
            Assert.AreEqual(cmdOne.Name, cmdTwo.Name);
            Assert.AreEqual(cmdOne.Value, cmdTwo.Value);
        }

        [Test, RequiresSTA]
        public void TestCreateCustomNodeCommand()
        {
            Guid modelGuid = Guid.NewGuid();
            string name = randomizer.Next().ToString();
            string category = randomizer.Next().ToString();
            string description = randomizer.Next().ToString();
            bool makeCurrent = randomizer.Next(2) == 0;

            var cmdOne = new DynamoViewModel.CreateCustomNodeCommand(
                modelGuid, name, category, description, makeCurrent);
            var cmdTwo = DuplicateAndCompare(cmdOne);

            Assert.AreEqual(cmdOne.NodeId, cmdTwo.NodeId);
            Assert.AreEqual(cmdOne.Name, cmdTwo.Name);
            Assert.AreEqual(cmdOne.Category, cmdTwo.Category);
            Assert.AreEqual(cmdOne.Description, cmdTwo.Description);
            Assert.AreEqual(cmdOne.MakeCurrent, cmdTwo.MakeCurrent);
        }
        [Test]
        public void TestCustomNode()
        {
            RunCommandsFromFile("TestCustomNode.xml");
            var workspaces = this.Controller.DynamoModel.Workspaces;
            Assert.IsNotNull(workspaces);
            Assert.AreEqual(2, workspaces.Count); // 1 custom node + 1 home space

            // 1 custom node + 1 number node
            Assert.AreEqual(1, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var customWorkspace = workspaces[1];
            Assert.IsNotNull(customWorkspace);

            // 1 inputs + 1 output 
            Assert.AreEqual(1, customWorkspace.Connectors.Count);
            Assert.AreEqual(2, customWorkspace.Nodes.Count);

            var node = customWorkspace.Nodes[0];
            var outports = node.OutPorts;

            AssertPreviewValue("04f6dab5-0a0b-4563-9f20-d0e58fcae7a5", 1.0);
        }
        [Test]
        public void TestCustomNodeUI()
        {
            RunCommandsFromFile("CustomNodeUI.xml", false, (commandTag) =>
            {
                var workspaces = Controller.DynamoModel.Workspaces;

                if (commandTag == "FirstRun")
                {
                    Assert.IsNotNull(workspaces);
                    Assert.AreEqual(2, workspaces.Count); // 1 custom node + 1 home space

                    // 1 custom node + 1 number node
                    Assert.AreEqual(2, workspace.Connectors.Count);
                    Assert.AreEqual(3, workspace.Nodes.Count);

                    var customWorkspace = workspaces[1];
                    Assert.IsNotNull(customWorkspace);

                    // 2 inputs + 1 output 
                    Assert.AreEqual(3, customWorkspace.Connectors.Count);
                    Assert.AreEqual(4, customWorkspace.Nodes.Count);

                    var node = GetNode("6cec1997-ed61-4277-a1a8-3f3e4eb4321d") as NodeModel;
                    Assert.AreEqual(2, node.InPorts.Count);
                    Assert.AreEqual(1, node.OutPorts.Count);

                    AssertPreviewValue("6cec1997-ed61-4277-a1a8-3f3e4eb4321d", 7.6);

                }
                else if (commandTag == "SecondRun")
                {

                    Assert.IsNotNull(workspaces);
                    Assert.AreEqual(2, workspaces.Count); // 1 custom node + 1 home space

                    // 1 custom node + 1 number node
                    Assert.AreEqual(3, workspace.Connectors.Count);
                    Assert.AreEqual(4, workspace.Nodes.Count);

                    var customWorkspace = workspaces[1];
                    Assert.IsNotNull(customWorkspace);

                    // 2 inputs + 1 output 
                    Assert.AreEqual(2, customWorkspace.Connectors.Count);
                    Assert.AreEqual(1, customWorkspace.Nodes.Count);
                    var node = GetNode("6cec1997-ed61-4277-a1a8-3f3e4eb4321d") as NodeModel;
                    Assert.AreEqual(3, node.InPorts.Count);
                    Assert.AreEqual(1, node.OutPorts.Count);
                    AssertPreviewValue("6cec1997-ed61-4277-a1a8-3f3e4eb4321d", 11.5);

                }


            });
        }
        [Test]
        public void Deffect_CN_1143()
        {
            // modify the name of the input node
            RunCommandsFromFile("Deffect_CN_1143.xml", false, (commandTag) =>
            {
                var workspaces = Controller.DynamoModel.Workspaces;

                if (commandTag == "FirstRun")
                {
                    Assert.IsNotNull(workspaces);
                    Assert.AreEqual(2, workspaces.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);
                    Assert.AreEqual(1, workspace.Nodes.Count);

                    var customWorkspace = workspaces[1];
                    Assert.IsNotNull(customWorkspace);

                    Assert.AreEqual(2, customWorkspace.Connectors.Count);
                    Assert.AreEqual(1, customWorkspace.Nodes.Count);

                    var node = GetNode("6cec1997-ed61-4277-a1a8-3f3e4eb4321d") as NodeModel;


                }
                else if (commandTag == "SecondRun")
                {

                    Assert.IsNotNull(workspaces);
                    Assert.AreEqual(2, workspaces.Count);


                    Assert.AreEqual(2, workspace.Connectors.Count);
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    var customWorkspace = workspaces[1];
                    Assert.IsNotNull(customWorkspace);
                    Assert.AreEqual(2, customWorkspace.Connectors.Count);
                    Assert.AreEqual(1, customWorkspace.Nodes.Count);




                }


            });
        }
        [Test]
        public void TestDeffect_CN_2144()
        {
            RunCommandsFromFile("Deffect_CN_2144.xml", false, (commandTag) =>
            {
                var workspaces = Controller.DynamoModel.Workspaces;

                if (commandTag == "FirstRun")
                {
                    Assert.IsNotNull(workspaces);
                    Assert.AreEqual(2, workspaces.Count); // 1 custom node + 1 home space

                    // 1 custom node + 1 number node
                    Assert.AreEqual(1, workspace.Connectors.Count);
                    Assert.AreEqual(2, workspace.Nodes.Count);

                    var customWorkspace = workspaces[1];
                    Assert.IsNotNull(customWorkspace);

                    // 2 inputs + 1 output 
                    Assert.AreEqual(2, customWorkspace.Connectors.Count);
                    Assert.AreEqual(3, customWorkspace.Nodes.Count);

                    var node = GetNode("6cec1997-ed61-4277-a1a8-3f3e4eb4321d") as NodeModel;

                    AssertPreviewValue("6cec1997-ed61-4277-a1a8-3f3e4eb4321d", 1);

                }
                else if (commandTag == "SecondRun")
                {

                    Assert.IsNotNull(workspaces);
                    Assert.AreEqual(2, workspaces.Count); // 1 custom node + 1 home space

                    // 1 custom node + 1 number node
                    Assert.AreEqual(1, workspace.Connectors.Count);
                    Assert.AreEqual(2, workspace.Nodes.Count);

                    var customWorkspace = workspaces[1];
                    Assert.IsNotNull(customWorkspace);

                    // 2 inputs + 1 output 
                    Assert.AreEqual(2, customWorkspace.Connectors.Count);
                    Assert.AreEqual(3, customWorkspace.Nodes.Count);
                    var node = GetNode("6cec1997-ed61-4277-a1a8-3f3e4eb4321d") as NodeModel;
                    Assert.AreEqual(2, node.InPorts.Count);
                    Assert.AreEqual(3, node.OutPorts.Count);
                    AssertPreviewValue("6cec1997-ed61-4277-a1a8-3f3e4eb4321d", 1);

                }


            });
        }
        [Test, RequiresSTA]
        public void TestSwitchTabCommand()
        {
            var cmdOne = new DynamoViewModel.SwitchTabCommand(randomizer.Next());
            var cmdTwo = DuplicateAndCompare(cmdOne);
            Assert.AreEqual(cmdOne.TabIndex, cmdTwo.TabIndex);
        }

        #endregion

        #region General Node Operations Test Cases

        [Test, RequiresSTA, Category("Failing")]
        public void MultiPassValidationSample()
        {
            RunCommandsFromFile("MultiPassValidationSample.xml", false, (commandTag) =>
            {
                if (commandTag == "InitialRun")
                {
                    AssertPreviewValue("c8d1655c-f4f4-41d1-bd5b-7ad39fc04118", 10);
                    AssertPreviewValue("0f2ef49a-eff4-445a-987b-9417b1a52cc5", 20);
                    AssertPreviewValue("e0556feb-95d9-4043-945b-f83aed25ef02", 30);
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("c8d1655c-f4f4-41d1-bd5b-7ad39fc04118", 2);
                    AssertPreviewValue("0f2ef49a-eff4-445a-987b-9417b1a52cc5", 3);
                    AssertPreviewValue("e0556feb-95d9-4043-945b-f83aed25ef02", 5);
                }
            });
        }

        [Test, RequiresSTA, Category("Failing")]
        public void TestModifyPythonNodes()
        {
            RunCommandsFromFile("ModifyPythonNodes.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var python = GetNode("6f580b72-6aeb-4af2-b28b-a2e5b634721b") as PythonNode;
            var pvarin = GetNode("f0fc1dea-3874-40a0-a532-90c0ee10f437") as PythonNode;

            Assert.AreEqual("# Modification 3", python.Script);
            Assert.AreEqual("# Modification 4", pvarin.Script);
        }

        [Test, RequiresSTA, Category("Failing")]
        public void TestModifyPythonNodesUndo()
        {
            RunCommandsFromFile("ModifyPythonNodesUndo.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var python = GetNode("6f580b72-6aeb-4af2-b28b-a2e5b634721b") as PythonNode;
            var pvarin = GetNode("f0fc1dea-3874-40a0-a532-90c0ee10f437") as PythonNode;

            Assert.AreEqual("# Modification 1", python.Script);
            Assert.AreEqual("# Modification 2", pvarin.Script);
        }

        [Test, RequiresSTA, Category("Failing")]
        public void TestModifyPythonNodesUndoRedo()
        {
            RunCommandsFromFile("ModifyPythonNodesUndoRedo.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var python = GetNode("6f580b72-6aeb-4af2-b28b-a2e5b634721b") as PythonNode;
            var pvarin = GetNode("f0fc1dea-3874-40a0-a532-90c0ee10f437") as PythonNode;

            Assert.AreEqual("# Modification 3", python.Script);
            Assert.AreEqual("# Modification 4", pvarin.Script);
        }

        [Test, Category("Failing")]
        public void CreateAndUseCustomNode()
        {
            RunCommandsFromFile("CreateAndUseCustomNode.xml");
            var workspaces = this.Controller.DynamoModel.Workspaces;
            Assert.IsNotNull(workspaces);
            Assert.AreEqual(2, workspaces.Count); // 1 custom node + 1 home space

            // 1 custom node + 3 number nodes + 1 watch node
            Assert.AreEqual(4, workspace.Connectors.Count);
            Assert.AreEqual(5, workspace.Nodes.Count);

            var customWorkspace = workspaces[1];
            Assert.IsNotNull(customWorkspace);

            // 3 inputs + 1 output + 1 addition + 1 multiplication
            Assert.AreEqual(5, customWorkspace.Connectors.Count);
            Assert.AreEqual(6, customWorkspace.Nodes.Count);

            AssertPreviewValue("345cd2d4-5f3b-4eb0-9d5f-5dd90c5a7493", 36.0);
        }

        #endregion

        #region Private Helper Methods

        protected ModelBase GetNode(string guid)
        {
            Guid id = Guid.Parse(guid);
            return Controller.DynamoModel.CurrentWorkspace.GetModelInternal(id);
        }

        /// <summary>
        /// Call this method to load custom nodes from their file paths. This 
        /// call, if made, must precede the call to RunCommandsFromFile. This 
        /// call cannot be made more than once for a single test case. If more 
        /// than one custom node files are needed for the test case, they must 
        /// be specified in the same call.
        /// </summary>
        /// <param name="customNodeFilePaths">And array of custom node file paths.
        /// This array cannot be null or empty.</param>
        /// 
        protected void LoadCustomNodes(string[] customNodeFilePaths)
        {
            if (customNodeFilePaths == null || (customNodeFilePaths.Length <= 0))
            {
                var message = "Argument must be one or more valid file paths";
                throw new ArgumentException(message);
            }

            if (this.customNodesToBeLoaded != null)
                throw new InvalidOperationException("LoadCustomNodes called twice");

            if (this.Controller != null)
            {
                var message = "'LoadCustomNodes' should be called before 'RunCommandsFromFile'";
                throw new InvalidOperationException(message);
            }

            var fileList = new List<string>();
            foreach (var customNodeFilePath in customNodeFilePaths)
            {
                if (File.Exists(customNodeFilePath) != false)
                {
                    fileList.Add(customNodeFilePath);
                    continue;
                }

                var message = "Custom node file not found";
                throw new System.IO.FileNotFoundException(message, customNodeFilePath);
            }

            this.customNodesToBeLoaded = fileList;
        }

        protected void RunCommandsFromFile(string commandFileName,
            bool autoRun = false, CommandCallback commandCallback = null)
        {
            string commandFilePath = DynamoTestUI.GetTestDirectory(ExecutingDirectory);
            commandFilePath = Path.Combine(commandFilePath, @"core\recorded\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            if (this.Controller != null)
            {
                var message = "Multiple DynamoController detected!";
                throw new InvalidOperationException(message);
            }

            // Create the controller to run alongside the view.
            this.Controller = DynamoController.MakeSandbox(commandFilePath);
            var controller = this.Controller;
            controller.DynamoViewModel.DynamicRunEnabled = autoRun;
            DynamoController.IsTestMode = true;

            // Load all custom nodes if there is any specified for this test.
            if (this.customNodesToBeLoaded != null)
            {
                foreach (var customNode in this.customNodesToBeLoaded)
                {
                    if (controller.CustomNodeManager.AddFileToPath(customNode) == null)
                    {
                        throw new System.IO.FileFormatException(string.Format(
                            "Failed to load custom node: {0}", customNode));
                    }
                }
            }

            RegisterCommandCallback(commandCallback);

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

        private void RegisterCommandCallback(CommandCallback commandCallback)
        {
            if (commandCallback == null)
                return;

            if (this.commandCallback != null)
                throw new InvalidOperationException("RunCommandsFromFile called twice");

            this.commandCallback = commandCallback;
            var automation = this.Controller.DynamoViewModel.Automation;
            automation.PlaybackStateChanged += OnAutomationPlaybackStateChanged;
        }

        private void OnAutomationPlaybackStateChanged(object sender, PlaybackStateChangedEventArgs e)
        {
            if (e.OldState == AutomationSettings.State.Paused)
            {
                if (e.NewState == AutomationSettings.State.Playing)
                {
                    // Call back to the delegate registered by the test case. We
                    // only handle command transition from Paused to Playing. Note 
                    // that "commandCallback" is not checked against "null" value 
                    // because "OnAutomationPlaybackStateChanged" would not have 
                    // been called if the "commandCallback" was not registered.
                    // 
                    this.commandCallback(e.NewTag);
                }
            }
        }

        private CmdType DuplicateAndCompare<CmdType>(CmdType command)
            where CmdType : DynamoViewModel.RecordableCommand
        {
            Assert.IsNotNull(command); // Ensure we have an input command.

            // Serialize the command into an XmlElement.
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement element = command.Serialize(xmlDocument);
            Assert.IsNotNull(element);

            // Deserialized the XmlElement into a new instance of the command.
            var duplicate = DynamoViewModel.RecordableCommand.Deserialize(element);
            Assert.IsNotNull(duplicate);
            Assert.IsTrue(duplicate is CmdType);
            return duplicate as CmdType;
        }

        #endregion
    }

    
    class RecordedTestsDSEngine : RecordedTests
    {
        #region Basic CodeBlockNode Test Cases

        [Test, RequiresSTA, Category("Failing")]
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
        [Test, RequiresSTA, Category("Failing")]
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
        [Test, RequiresSTA, Category("Failing")]
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
        [Test, RequiresSTA, Category("Failing")]
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
            //RunCommandsFromFile("TestConvertAllNodesToCodeUndo.xml");

            ////Check the nodes
            //var nodes = workspaceViewModel.Nodes;
            //Assert.NotNull(nodes);
            //Assert.AreEqual(4, nodes.Count);

            ////Check the connectors
            //var connectors = workspaceViewModel.Connectors;
            //Assert.NotNull(connectors);
            //Assert.AreEqual(2, connectors.Count);

            ////Check that there is no CBN
            //var cbn = GetNode("37fade4a-e7ad-43ae-8b6f-27dacb17c1c5") as CodeBlockNodeModel;
            //Assert.AreEqual(null, cbn);

            //var addNode = workspaceViewModel._model.Nodes.Where(x => x is DSFunction).First() as DSFunction;
            //Assert.NotNull(addNode);

            //var numberList = workspaceViewModel._model.Nodes.Where(x => x is DoubleInput).ToList<NodeModel>();
            //Assert.AreEqual(3, numberList.Count);

            Assert.Inconclusive("Porting : DoubleInput");
        }

        /// <summary>
        /// Ensures that redo works for NodeToCode by converting a set of nodes to
        /// code and then undoing and redoing it again.
        /// </summary>
        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
        public void TestUpdateNodeCaptions_DS()
        {
            RunCommandsFromFile("TestUpdateNodeCaptions_DS.xml");
            Assert.AreEqual(0, workspace.Connectors.Count);
            Assert.AreEqual(2, workspace.Nodes.Count);

            var cbn = GetNode("5cf9dff2-4a3e-428a-a98a-60d0de0d323e") as CodeBlockNodeModel;

            Assert.IsNotNull(cbn);

            Assert.AreEqual("CBN", cbn.NickName);
        }

        [Test, RequiresSTA]
        public void ReExecuteASTTest()
        {
            RunCommandsFromFile("ReExecuteASTTest.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    AssertPreviewValue("cdaf568a-e830-4eb0-bce0-983a7a0903e1", 1);

                }
                else if (commandTag == "SecondRun")
                {

                    AssertPreviewValue("cdaf568a-e830-4eb0-bce0-983a7a0903e1", 1);

                }
            });
        }

        #endregion

        #region Defect Verifications Test Cases


        [Test, RequiresSTA]
        public void Defect_MAGN_1956()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1956
            RunCommandsFromFile("Defect_MAGN_1956.xml");
            AssertPreviewValue("bbec3d26-e220-4b55-9da6-ca1f37a55d7f", -10);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_159()
        {
            //// Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-159

            //RunCommandsFromFile("Defect_MAGN_159.xml", true);

            //Assert.AreEqual(1, workspace.Nodes.Count);
            //Assert.AreEqual(0, workspace.Connectors.Count);

            //var number1 = GetNode("045decd1-7454-4b85-b92e-d59d35f31ab2") as DoubleInput;

            Assert.Inconclusive("Porting : DoubleInput");
        }

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_225_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-225

            RunCommandsFromFile("Defect_MAGN_225_DS.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);

        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_397_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-397
            RunCommandsFromFile("Defect_MAGN_397_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_429_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-429
            RunCommandsFromFile("Defect_MAGN_429_DS.xml");

            Assert.AreEqual(0, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_478_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-478

            // Same XML can be used for this test case as well.
            RunCommandsFromFile("Defect_MAGN_478.xml");

            Assert.AreEqual(1, workspace.Notes.Count);
        }

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_520_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520
            RunCommandsFromFile("Defect_MAGN_520_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_520_WithCrossSelection_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520

            // Same XML can be used for this test case as well.
            RunCommandsFromFile("Defect_MAGN_520_WithCrossSelection.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_581_DS()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-581
            RunCommandsFromFile("Defect_MAGN_581_DS.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);
        }

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_775()
        {
            // The third undo operation should not crash.
            RunCommandsFromFile("Defect_MAGN_775.xml");
            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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

        [Test, RequiresSTA, Category("Failing")]
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
        [Test, RequiresSTA, Category("Failing")]
        public void TestCBNWithNodeToCode()
        {
            // Run playback is recorded in command file
            RunCommandsFromFile("TestCBNOperationWithoutNodeToCode.xml");
            AssertValue("c_089fbe21a34547759592b683550558dd", 8);

            // Reset current test case
            Exit();
            Start();

            // Run playback is recorded in command file
            RunCommandsFromFile("TestCBNOperationWithNodeToCode.xml");
            AssertValue("c_089fbe21a34547759592b683550558dd", 8);

            // Reset current test case
            Exit();
            Start();

            // Run playback is recorded in command file
            RunCommandsFromFile("TestCBNOperationWithNodeToCodeUndo.xml");
            AssertValue("c_089fbe21a34547759592b683550558dd", 8);
        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_902()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-902
            RunCommandsFromFile("Defect_MAGN_902.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);

            ////Check the CBN for input and output ports count
            //var cbn = GetNode("09eb3645-f5e9-4186-8543-2195e7740eb2") as CodeBlockNodeModel;
            //Assert.AreNotEqual(ElementState.Error, cbn.State);
            //Assert.AreEqual(1, cbn.OutPorts.Count);
            //Assert.AreEqual(0, cbn.InPorts.Count);

            ////Check the position of ports
            //Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            //Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_422()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-422
            RunCommandsFromFile("Defect_MAGN_422.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("0a79dc3a-a37c-40a0-a631-eae730e8d3e2") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports and their names
            Assert.AreEqual("x", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(4, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("y", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(40, cbn.OutPorts[1].MarginThickness.Top);

        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_422_1()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-422
            RunCommandsFromFile("Defect_MAGN_422_1.xml");

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            //Check the CBN for input and output ports count
            var cbn = GetNode("811be42d-b44b-434a-ad6f-ae2c8d5309b1") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(2, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            //Check the position of ports and their names
            Assert.AreEqual("a", cbn.OutPorts[0].ToolTipContent);
            Assert.AreEqual(44, cbn.OutPorts[0].MarginThickness.Top);

            Assert.AreEqual("b", cbn.OutPorts[1].ToolTipContent);
            Assert.AreEqual(40, cbn.OutPorts[1].MarginThickness.Top);

        }

        [Test]
        public void DS_FunctionRedef01()
        {
            // test for function redefinition - evalaute function
            RunCommandsFromFile("Function_redef01.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            var cbn = GetNode("babc4816-96e6-495c-8489-7a650d1bfb25") as CodeBlockNodeModel;
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            
            AssertValue("e_babc481696e6495c84897a650d1bfb25", 1);
            AssertValue("p_d4d53e201514434983e17cb5c533a3e0", 0);
            
            Exit();
            Start();
            
            // redefine function - test if the CBN reexecuted
            RunCommandsFromFile("Function_redef01a.xml");
            
            AssertValue("e_babc481696e6495c84897a650d1bfb25", 3);
            AssertValue("p_d4d53e201514434983e17cb5c533a3e0", 0);
        }

        [Test]
        public void DS_FunctionRedef02()
        {
            // test for function redefinition - evalaute function
            RunCommandsFromFile("Function_redef02.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            AssertValue("d_0ce2353ce5d6445f83b72db7e3861ce0", 1);
            AssertValue("p_c9827e41855647f68e9d6c600a2e45ee", 0);

            Exit();
            Start();

            // redefine function call - CBN with function definition is not expected to be executed
            RunCommandsFromFile("Function_redef02a.xml");

            AssertValue("d_c553dcff09fa4ff9b1fb04c95f1ce2d8", 3);
            AssertValue("p_ed9c9950a1dc4487b1269a07d999d8a8", 0);
        }

        [Test]
        public void DS_FunctionRedef03()
        {
            // test for function redefinition - evalaute function
            RunCommandsFromFile("Function_redef03.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            AssertValue("c_f34e01e225e446349eb8e815e8ee580d", 0);
            AssertValue("d_f34e01e225e446349eb8e815e8ee580d", 1);

            Exit();
            Start();

            // redefine function call - CBN with function definition is not expected to be executed
            RunCommandsFromFile("Function_redef03a.xml");

            AssertValue("c_f34e01e225e446349eb8e815e8ee580d", 0);
            AssertValue("d_f34e01e225e446349eb8e815e8ee580d", 1);
        }

        [Test]
        public void DS_FunctionRedef04()
        {
            // test for function redefinition - evalaute function
            RunCommandsFromFile("Function_redef04.xml");

            Assert.AreEqual(4, workspace.Nodes.Count);

            AssertValue("c_275d7a3d2b984f0e808d2aba03c6ff4f", 1);
            AssertValue("b_9b638b99d63145838b82662a60cdf6bc", 0);
            
            Exit();
            Start();
            
            // redefine function call - change type of argument
            RunCommandsFromFile("Function_redef04a.xml");
            
            AssertValue("c_275d7a3d2b984f0e808d2aba03c6ff4f", new object[] { 1, 2, 3 });
            AssertValue("b_9b638b99d63145838b82662a60cdf6bc", 0);
        }

        [Test, RequiresSTA, Category("Failing")]
        public void MethodResolutionFailRedef_MAGN_2262()
        {
            RunCommandsFromFile("MethodResolutionFailRedef_MAGN_2262.xml", false, (commandTag) =>
            {
                if (commandTag == "Tag-204ca7e6")
                {
                    AssertPreviewValue("53cd0201-273c-43f4-8815-8531db75d68c", null);

                }
                else if (commandTag == "Tag-2f9b6919")
                {
                    AssertPreviewValue("53cd0201-273c-43f4-8815-8531db75d68c", true);

                }
            });
        }


        [Test, RequiresSTA]
        public void TestCallsiteMapModifyFunctionParamValue()
        {
            Guid callsiteGuidFirstCall = Guid.Empty;
            Guid callsiteGuidSecondCall = Guid.Empty;
            Guid FunctionCallNodeGuid = new Guid("22939bf5-50bc-4aa3-9c91-0dc5b5017252");

            RunCommandsFromFile("TestCallsiteMapModifyFunctionParamValue.xml", false, (commandTag) =>
            {
                ProtoCore.Core core = Controller.EngineController.LiveRunnerCore;
                if (commandTag == "ModifyX_FirstTime")
                {
                    // There must only be 1 callsite at this point
                    Assert.AreEqual(1, core.CallSiteToNodeMap.Count);

                    // Verify that the nodemap contains the node guid
                    bool containsNodeGuid = core.CallSiteToNodeMap.ContainsValue(FunctionCallNodeGuid);
                    Assert.AreEqual(true, containsNodeGuid);

                    // Get the callsite guid
                    foreach (KeyValuePair<Guid, Guid> kvp in core.CallSiteToNodeMap)
                    {
                        callsiteGuidFirstCall = kvp.Key;
                    }
                }
                else if (commandTag == "ModifyX_SecondTime")
                {
                    // There must only be 1 callsite at this point
                    Assert.AreEqual(1, core.CallSiteToNodeMap.Count);

                    // Verify that the nodemap contains the node guid
                    bool containsNodeGuid = core.CallSiteToNodeMap.ContainsValue(FunctionCallNodeGuid);
                    Assert.AreEqual(true, containsNodeGuid);

                    // Get the callsite guid
                    foreach (KeyValuePair<Guid, Guid> kvp in core.CallSiteToNodeMap)
                    {
                        callsiteGuidSecondCall = kvp.Key;
                    }

                    // The callsite guid must match 
                    // This means that that the callsite was cached and reused
                    Assert.AreEqual(callsiteGuidFirstCall, callsiteGuidSecondCall);
                }

            });
        }

        [Test, RequiresSTA]
        public void Deffect_1412CreateList()
        {
            // This is a UI test to test for interaction crashes the application

            RunCommandsFromFile("Deffect_1412CreateList.xml");
            Assert.AreEqual(4, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);
        }
        [Test, RequiresSTA]
        public void Deffect_1344PythonEditor()
        {
            // This is a UI test to test for interaction crashes the application

            RunCommandsFromFile("Deffect_1344PythonEditor.xml");
            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);
        }
        [Test, RequiresSTA]
        public void Deffect_2208Delete_CBN()
        {
            // This is a UI test to test for interaction crashes the application

            RunCommandsFromFile("Defect_MAGN_2208.xml");
            Assert.AreEqual(0, workspace.Nodes.Count);
        }
        [Test, RequiresSTA]
        public void Deffect_2201Watch_CBN()
        {
            RunCommandsFromFile("Defect_MAGN_2201.xml");
            Assert.AreEqual(3, workspace.Nodes.Count);
        }
        [Test, RequiresSTA]
        public void Deffect_747MultiReference()
        {
            RunCommandsFromFile("defect_MAGN_747.xml", true);
            Assert.AreEqual(1, workspace.Nodes.Count);
            AssertPreviewValue("a76409a1-1280-428c-9cf7-16580c48ff96", 1);

        }


        [Test, RequiresSTA]
        public void TestCallsiteMapModifyInputConnection()
        {
            Guid callsiteGuidFirstCall = Guid.Empty;
            Guid callsiteGuidSecondCall = Guid.Empty;
            Guid FunctionCallNodeGuid = new Guid("16e960e5-8a24-44e7-ac81-3759aaf11d25");

            RunCommandsFromFile("TestCallsiteMapModifyModifyInputConnection.xml", false, (commandTag) =>
            {
                ProtoCore.Core core = Controller.EngineController.LiveRunnerCore;
                if (commandTag == "ModifyX_FirstTime")
                {
                    // There must only be 1 callsite at this point
                    Assert.AreEqual(1, core.CallSiteToNodeMap.Count);

                    // Verify that the nodemap contains the node guid
                    bool containsNodeGuid = core.CallSiteToNodeMap.ContainsValue(FunctionCallNodeGuid);
                    Assert.AreEqual(true, containsNodeGuid);

                    // Get the callsite guid
                    foreach (KeyValuePair<Guid, Guid> kvp in core.CallSiteToNodeMap)
                    {
                        callsiteGuidFirstCall = kvp.Key;
                    }
                }
                else if (commandTag == "ModifyX_SecondTime")
                {
                    // There must only be 1 callsite at this point
                    Assert.AreEqual(1, core.CallSiteToNodeMap.Count);

                    // Verify that the nodemap contains the node guid
                    bool containsNodeGuid = core.CallSiteToNodeMap.ContainsValue(FunctionCallNodeGuid);
                    Assert.AreEqual(true, containsNodeGuid);

                    // Get the callsite guid
                    foreach (KeyValuePair<Guid, Guid> kvp in core.CallSiteToNodeMap)
                    {
                        callsiteGuidSecondCall = kvp.Key;
                    }

                    // The callsite guid must match 
                    // This means that that the callsite was cached and reused
                    Assert.AreEqual(callsiteGuidFirstCall, callsiteGuidSecondCall);
                }

            });
        }

        [Test, RequiresSTA]

        //Details for steps can be found in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2521

        public void Defect_MAGN_2521()
        {
            RunCommandsFromFile("Defect_MAGN_2521.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "Point-10")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    AssertPreviewValue("c0a70160-fb7e-4441-a06e-1e783d7c0d92", 10);
                }
                else if (commandTag == "Point-2")
                {
                    AssertPreviewValue("c0a70160-fb7e-4441-a06e-1e783d7c0d92", 2);
                }
                else if (commandTag == "Point-12")
                {
                    Assert.AreEqual(1, workspace.Connectors.Count);
                    AssertPreviewValue("c0a70160-fb7e-4441-a06e-1e783d7c0d92", 12);
                }
                else if (commandTag == "Point--5")
                {
                    AssertPreviewValue("c0a70160-fb7e-4441-a06e-1e783d7c0d92", -5);
                }
            });
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2378()
        {
            // this is using CBN. 
            // more details available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2378

            RunCommandsFromFile("Defect_MAGN_2378.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "WithWarning")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    NodeModel nodeModel = workspace.NodeFromWorkspace("f4a9a71a-168b-4985-a575-d16faa2de9ca");
                    Assert.AreEqual(ElementState.Warning, nodeModel.State);

                }
                else if (commandTag == "WithoutWarning")
                {
                    NodeModel nodeModel = workspace.NodeFromWorkspace("f4a9a71a-168b-4985-a575-d16faa2de9ca");
                    Assert.AreNotEqual(ElementState.Warning, nodeModel.State);
                }
            });
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2378_AnotherScenario()
        {
            // this is using Point.ByCoordinates node.
            // more details available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2378

            RunCommandsFromFile("Defect_MAGN_2378_Another.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "WithWrongInput")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(3, workspace.Connectors.Count);

                    NodeModel nodeModel = workspace.NodeFromWorkspace("7fec1085-4818-41fa-adad-7b1dd71a5195");
                    Assert.AreEqual(ElementState.Warning, nodeModel.State);

                }
                else if (commandTag == "WithCorrectInput")
                {
                    Assert.AreEqual(4, workspace.Nodes.Count);
                    Assert.AreEqual(4, workspace.Connectors.Count);

                    NodeModel nodeModel = workspace.NodeFromWorkspace("7fec1085-4818-41fa-adad-7b1dd71a5195");
                    Assert.AreNotEqual(ElementState.Warning, nodeModel.State);

                    AssertPreviewValue("5778488b-4ab2-484a-b92e-79eb016cb776", 0);

                }
            });
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2100()
        {
            // more details available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2100

            RunCommandsFromFile("Defect_MAGN_2100.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    AssertPreviewValue("3f309016-7b00-4487-9b68-f0640e892d39", 10);

                }

            });

            NodeModel nodeModel = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace(
                "3f309016-7b00-4487-9b68-f0640e892d39");

            Assert.AreNotEqual(ElementState.Warning, nodeModel.State);

            Assert.IsNotNull(nodeModel.CachedValue.Data);

            AssertPreviewValue("3f309016-7b00-4487-9b68-f0640e892d39", 11);

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2102()
        {
            // more details available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2102

            RunCommandsFromFile("Defect_MAGN_2102.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "Start")
                {
                    Assert.AreEqual(0, workspace.Connectors.Count);
                    Assert.AreEqual(2, workspace.Nodes.Count);

                    var node1 = GetNode("37da4958-1b88-408b-b09d-3deba0ba3835");
                    var node2 = GetNode("b12ce9c8-8c23-43c4-987d-759c6f623998");

                    Assert.NotNull(node1 as DSCoreNodesUI.DummyNode);
                    Assert.NotNull(node2 as DSCoreNodesUI.DummyNode);
                }
                else if (commandTag == "Delete1")
                {
                    Assert.AreEqual(1, workspace.Nodes.Count);
                }
                else if (commandTag == "UndoDelete1")
                {
                    Assert.AreEqual(2, workspace.Nodes.Count);
                }
                else if (commandTag == "RedoDelete1")
                {
                    Assert.AreEqual(1, workspace.Nodes.Count);
                }
                else if (commandTag == "Delete2")
                {
                    Assert.AreEqual(0, workspace.Nodes.Count);
                }
                else if (commandTag == "UndoDelete2")
                {
                    Assert.AreEqual(1, workspace.Nodes.Count);
                }
            });
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2272()
        {
            // more details available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2272
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2545

            RunCommandsFromFile("Defect_MAGN_2272.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    AssertPreviewValue("d480bc8e-4a77-44ea-ab07-52070ec6a5b6", 6);

                }
                else if (commandTag == "SecondRun")
                {

                    AssertPreviewValue("d480bc8e-4a77-44ea-ab07-52070ec6a5b6", 7);

                }
                else if (commandTag == "LastRun")
                {
                    AssertPreviewValue("d480bc8e-4a77-44ea-ab07-52070ec6a5b6", 11);

                }

            });


        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2528()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2528
            RunCommandsFromFile("Defect_MAGN_2528.xml", true);

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            //Check the CBN for error
            var cbn = GetNode("10f928da-a6ef-4235-b84b-883f66e26017") as CodeBlockNodeModel;
            Assert.AreEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(0, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);
        }


        [Test, RequiresSTA]
        public void Defect_MAGN_2453()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2453

            RunCommandsFromFile("Defect_MAGN_2453.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    AssertPreviewValue("ab11bb36-b428-4297-ac25-7afeeefff487", new int[] { 0, 2 });
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("ab11bb36-b428-4297-ac25-7afeeefff487", new int[] { 0, 2, 4 });
                }
                else if (commandTag == "ThirdRun")
                {
                    AssertPreviewValue("ab11bb36-b428-4297-ac25-7afeeefff487", new int[] { 2, 3, 4 });
                }
                else if (commandTag == "LastRun")
                {
                    AssertPreviewValue("ab11bb36-b428-4297-ac25-7afeeefff487", new int[] { 4, 5, 6 });
                }

            });


        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2593()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2593

            RunCommandsFromFile("Defect_MAGN_2593.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    AssertPreviewValue("2be171fb-2f81-4244-88ec-a8827a77e150", new int[] { 5 });
                }
                else if (commandTag == "SecondRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(3, workspace.Connectors.Count);

                    AssertPreviewValue("2be171fb-2f81-4244-88ec-a8827a77e150",
                        new int[] { 5, 5, 5 });
                }
                else if (commandTag == "ThirdRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(4, workspace.Nodes.Count);
                    Assert.AreEqual(5, workspace.Connectors.Count);

                    AssertPreviewValue("2be171fb-2f81-4244-88ec-a8827a77e150",
                        new int[] { 5, 5, 5, 6, 7 });
                }
                else if (commandTag == "FourthRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(4, workspace.Nodes.Count);
                    Assert.AreEqual(4, workspace.Connectors.Count);

                    AssertPreviewValue("2be171fb-2f81-4244-88ec-a8827a77e150",
                        new int[] { 5, 5, 5, 6 });
                }

                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(4, workspace.Nodes.Count);
                    Assert.AreEqual(3, workspace.Connectors.Count);

                    AssertPreviewValue("2be171fb-2f81-4244-88ec-a8827a77e150",
                        new int[] { 5, 5, 5 });
                }

            });


        }

        [Test, RequiresSTA]
        public void Defect_MAGN_3113()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3113

            RunCommandsFromFile("Defect_MAGN_3113.xml", true);
            var workspace = Controller.DynamoModel.CurrentWorkspace;

            // check for number of Nodes and Connectors
            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

            // Only in the UI it is showing {6, null}, but still this test make sense to add for 
            // tracking regression, if we get different output after undo/redo.
            AssertPreviewValue("d54551b2-5775-4b1e-a064-f8b9e0f2b3a0", new int[] { 6 });

        }


        [Test, RequiresSTA]
        public void Defect_MAGN_2373()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2373

            RunCommandsFromFile("Defect_MAGN_2373.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(10, workspace.Nodes.Count);
                    Assert.AreEqual(13, workspace.Connectors.Count);

                    AssertPreviewValue("6e2b9ef1-7749-40ec-9319-b43832c1891f",
                        new int[][] { new int[] { 2, 8, 9, 3 } });
                }
                else if (commandTag == "SecondRun")
                {
                    AssertPreviewValue("6e2b9ef1-7749-40ec-9319-b43832c1891f",
                        new int[][] { new int[] { 2, 8, 3, 9 } });
                }
                else if (commandTag == "LastRun")
                {
                    AssertPreviewValue("6e2b9ef1-7749-40ec-9319-b43832c1891f",
                        new int[][] { new int[] { 8, 2, 3, 9 } });
                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2563()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2563

            RunCommandsFromFile("Defect_MAGN_2563.xml", true);

            Assert.AreEqual(1, Controller.DynamoModel.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(0, Controller.DynamoModel.CurrentWorkspace.Connectors.Count);


            NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("aeed3ffe-7294-43a9-8a05-83b5ff05f527");

            Assert.AreEqual(ElementState.Warning, node.State);
        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2247()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2247

            RunCommandsFromFile("Defect_MAGN_2247.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(5, workspace.Nodes.Count);
                    Assert.AreEqual(4, workspace.Connectors.Count);

                    AssertPreviewValue("3f1e91ab-8b71-4626-b632-a18df1be036c", 23);
                }
                else if (commandTag == "SecondRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(5, workspace.Nodes.Count);
                    Assert.AreEqual(3, workspace.Connectors.Count);

                    AssertPreviewValue("3f1e91ab-8b71-4626-b632-a18df1be036c", 17);
                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(5, workspace.Nodes.Count);
                    Assert.AreEqual(4, workspace.Connectors.Count);

                    AssertPreviewValue("3f1e91ab-8b71-4626-b632-a18df1be036c", 21);
                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2311()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2311

            RunCommandsFromFile("Defect_MAGN_2311.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("d00ce832-8109-42d5-bcde-e7179a7bc5b6");

                    Assert.AreEqual(ElementState.Warning, node.State);
                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("d00ce832-8109-42d5-bcde-e7179a7bc5b6");

                    Assert.AreEqual(ElementState.Warning, node.State);
                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2279()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2279

            RunCommandsFromFile("Defect_MAGN_2279.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    AssertPreviewValue("55b87e32-7279-49bf-982c-91d06b349439",
                        new int[] { 0, 1, 2, 3 });
                }
                else if (commandTag == "SecondRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("bc2c4de8-43a1-4b36-b0d6-309423664089");

                    Assert.AreNotEqual(ElementState.Warning, node.State);
                    AssertPreviewValue("bc2c4de8-43a1-4b36-b0d6-309423664089",
                        new int[] { 0, 1, 2, 3 });
                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("bc2c4de8-43a1-4b36-b0d6-309423664089");

                    Assert.AreNotEqual(ElementState.Warning, node.State);

                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_3116()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3116

            RunCommandsFromFile("Defect_MAGN_3116.xml", false, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    AssertPreviewValue("bdfcf8ef-11fa-4881-9b65-73ca99bb2b58", 0);
                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("6e2644dc-3336-4a87-a97f-12b2aab14a6b");

                    Assert.AreNotEqual(ElementState.Warning, node.State);

                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_2290()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2290

            RunCommandsFromFile("Defect_MAGN_2290.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    AssertPreviewValue("826ba392-b385-4960-89cc-c076c3abffb0",
                        new int[] { 0, 1, 2, 3 });

                    AssertPreviewValue("8765fc5f-4edd-482c-8541-9acb6e39352c",
                        new int[] { 0, 1, 2, 3 });

                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(2, workspace.Connectors.Count);

                    NodeModel node = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("826ba392-b385-4960-89cc-c076c3abffb0");
                    Assert.AreNotEqual(ElementState.Warning, node.State);
                    AssertPreviewValue("826ba392-b385-4960-89cc-c076c3abffb0", new int[] { 0, 3 });

                    NodeModel node1 = Controller.DynamoModel.CurrentWorkspace.NodeFromWorkspace
                        ("8765fc5f-4edd-482c-8541-9acb6e39352c");
                    Assert.AreNotEqual(ElementState.Warning, node1.State);
                    AssertPreviewValue("8765fc5f-4edd-482c-8541-9acb6e39352c",
                        new int[] { 0, 1, 2, 3 });

                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_3166()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3166

            RunCommandsFromFile("Defect_MAGN_3166.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(3, workspace.Connectors.Count);

                    AssertPreviewValue("3df462b5-d4e6-4c28-9d1c-9bf6099ba77a",
                        new int[] { 1, 2, 1, 2 });

                    AssertPreviewValue("201c3f70-ed7b-4d8e-9022-a34c380d55a1",
                        new int[] { 1, 2, 1, 2 });
                }

                else if (commandTag == "SecondRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);
                }

                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(3, workspace.Nodes.Count);
                    Assert.AreEqual(3, workspace.Connectors.Count);

                    AssertPreviewValue("3df462b5-d4e6-4c28-9d1c-9bf6099ba77a",
                        new int[] { 1, 2, 1, 2 });

                    AssertPreviewValue("201c3f70-ed7b-4d8e-9022-a34c380d55a1",
                        new int[] { 1, 2, 1, 2 });
                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_3599()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3599

            RunCommandsFromFile("Defect_MAGN_3599.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                var cbn = GetNode("26d75d07-10d7-4517-83b8-0f45711706b2") as CodeBlockNodeModel;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(1, cbn.OutPorts.Count);
                    Assert.AreEqual(1, cbn.InPorts.Count);

                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(2, cbn.OutPorts.Count);
                    Assert.AreEqual(0, cbn.InPorts.Count);

                    AssertPreviewValue("26d75d07-10d7-4517-83b8-0f45711706b2", 3);

                }

            });

        }


        [Test, RequiresSTA]
        public void Defect_MAGN_3580()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3580

            RunCommandsFromFile("Defect_MAGN_3580.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                var cbn = GetNode("b9c2edd6-5d73-4243-90fb-2b608250adee") as CodeBlockNodeModel;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(1, cbn.OutPorts.Count);
                    Assert.AreEqual(1, cbn.InPorts.Count);

                }
                else if (commandTag == "SecondRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(1, cbn.OutPorts.Count);
                    Assert.AreEqual(1, cbn.InPorts.Count);
                    
                    // check preview value of CBN
                    AssertPreviewValue("b9c2edd6-5d73-4243-90fb-2b608250adee", false);

                }
                else if (commandTag == "ThirdRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(1, cbn.OutPorts.Count);
                    Assert.AreEqual(0, cbn.InPorts.Count);

                    // check preview value of CBN
                    AssertPreviewValue("b9c2edd6-5d73-4243-90fb-2b608250adee", true);

                }
                else if (commandTag == "FourthRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(1, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    // check preview value of CBN
                    AssertPreviewValue("8dd1d732-f5d7-42d9-aadb-c960ac5d868e", false);

                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(1, cbn.OutPorts.Count);
                    Assert.AreEqual(0, cbn.InPorts.Count);

                    // check preview value of CBN
                    AssertPreviewValue("b9c2edd6-5d73-4243-90fb-2b608250adee", true);

                }

            });

        }

        [Test, RequiresSTA]
        public void Defect_MAGN_3212()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3212

            RunCommandsFromFile("Defect_MAGN_3212.xml", true, (commandTag) =>
            {
                var workspace = Controller.DynamoModel.CurrentWorkspace;

                var cbn = GetNode("28029adc-b19d-414c-996c-545572aa9efc") as CodeBlockNodeModel;

                if (commandTag == "FirstRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(1, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(1, cbn.OutPorts.Count);
                    Assert.AreEqual(1, cbn.InPorts.Count);

                    AssertPreviewValue("28029adc-b19d-414c-996c-545572aa9efc", 8);

                }
                else if (commandTag == "LastRun")
                {
                    // check for number of Nodes and Connectors
                    Assert.AreEqual(2, workspace.Nodes.Count);
                    Assert.AreEqual(0, workspace.Connectors.Count);

                    //Check the CBN for input/output ports
                    Assert.AreNotEqual(ElementState.Error, cbn.State);
                    Assert.AreEqual(0, cbn.OutPorts.Count);
                    Assert.AreEqual(0, cbn.InPorts.Count);

                }

            });

        }


        #endregion

        #region Tests moved from FScheme

        [Test, Category("Failing")]
        public void Defect_MAGN_159_AnotherScenario()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-159

            RunCommandsFromFile("Defect_MAGN_159.xml", true);

            Assert.AreEqual(1, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            AssertPreviewValue("045decd1-7454-4b85-b92e-d59d35f31ab2", 8);
        }

        [Ignore]
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

        [Test, Category("Failing")]
        public void Defect_MAGN_164()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-164

            RunCommandsFromFile("Defect_MAGN_164.xml", true);

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

            AssertPreviewValue("428fc0eb-aacf-41ca-b4d9-d4152e945ad8", 10);

            AssertPreviewValue("635bd033-03f6-4b98-b03d-a5c3c2969607", 10);
        }

        [Test, Category("Failing")]
        public void Defect_MAGN_190()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-190
            RunCommandsFromFile("Defect_MAGN_190.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);

        }

        [Test, Category("Failing")]
        public void Defect_MAGN_225()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-225

            // TODO: Rename this XML to match the test case name.
            RunCommandsFromFile("TestConnectionReplacementUndo.xml");
            var nodes = workspaceViewModel.Nodes;

            Assert.NotNull(nodes);
            Assert.AreEqual(3, nodes.Count);
        }

        [Test, Category("Failing")]
        public void Defect_MAGN_397()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-397
            RunCommandsFromFile("Defect_MAGN_397.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);
        }

        [Test, Category("Failing")]
        public void Defect_MAGN_429()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-429
            RunCommandsFromFile("Defect_MAGN_429.xml");

            Assert.AreEqual(0, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);

        }

        [Test, Category("Failing")]
        public void Defect_MAGN_478()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-478
            RunCommandsFromFile("Defect_MAGN_478.xml");

            Assert.AreEqual(1, workspace.Notes.Count);
        }

        [Test, Category("Failing")]
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

        [Test, Category("Failing")]
        public void Defect_MAGN_520()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520
            RunCommandsFromFile("Defect_MAGN_520.xml", true);
            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(2, workspace.Connectors.Count);
            AssertPreviewValue("41f52d8e-1a88-4f09-a2f1-f14e61d81f2c", 4);
        }

        [Test, Category("Failing")]
        public void Defect_MAGN_520_WithCrossSelection()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-520
            RunCommandsFromFile("Defect_MAGN_520_WithCrossSelection.xml");

            Assert.AreEqual(3, workspace.Nodes.Count);
            Assert.AreEqual(0, workspace.Connectors.Count);
        }

        [Test]
        public void Defect_MAGN_57()
        {
            Assert.Inconclusive("Deprecated: Map");

            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-57
            RunCommandsFromFile("Defect_MAGN_57.xml");

            Assert.AreEqual(7, workspace.Nodes.Count);
            Assert.AreEqual(5, workspace.Connectors.Count);

        }

        [Test, RequiresSTA, Category("Failing")]
        public void Defect_MAGN_581()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-581
            RunCommandsFromFile("Defect_MAGN_581.xml");

            Assert.AreEqual(2, workspace.Nodes.Count);
            Assert.AreEqual(1, workspace.Connectors.Count);
        }

        [Test, Category("Failing")]
        public void ShiftSelectAllNode()
        {
            RunCommandsFromFile("ShiftSelectAllNode.xml");

            Assert.AreEqual(4, workspace.Nodes.Count);
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        [Test, Category("Failing")]
        public void TestCreateConnectors()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        [Test, Category("Failing")]
        public void TestCreateNodesAndRunExpression()
        {
            RunCommandsFromFile("CreateNodesAndRunExpression.xml");
            AssertPreviewValue("cf8c52b1-fbee-4674-ba73-6ee0d09463f2", 6);

        }

        [Test, Category("Failing")]
        public void TestCreateNodes()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            Assert.AreEqual(5, workspace.Nodes.Count);
        }

        [Test, Category("Failing")]
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

        [Test, Category("Failing")]
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

        [Test]
        public void TestUpdateNodeCaptions()
        {
            //RunCommandsFromFile("UpdateNodeCaptions.xml");
            //Assert.AreEqual(0, workspace.Connectors.Count);
            //Assert.AreEqual(1, workspace.Notes.Count);
            //Assert.AreEqual(2, workspace.Nodes.Count);

            //var number = GetNode("a9762506-2ab6-4b50-8166-138de5b0c704") as DoubleInput;
            //var note = GetNode("21c66403-d102-42bd-97ae-9e7b9c0b6e7d") as NoteModel;

            //Assert.IsNotNull(number);
            //Assert.IsNotNull(note);

            //Assert.AreEqual("Caption 1", number.NickName);
            //Assert.AreEqual("Caption 3", note.Text);

            Assert.Inconclusive("Porting : DoubleInput");
        }

        [Test]
        public void TestUpdateNodeContents()
        {
            //RunCommandsFromFile("UpdateNodeContents.xml", true);
            //Assert.AreEqual(2, workspace.Connectors.Count);
            //Assert.AreEqual(3, workspace.Nodes.Count);

            //var number = GetNode("31f48bb5-4bdf-4066-b343-5df0f6f4337f") as DoubleInput;
            //var slider = GetNode("ff4d4e43-8932-4588-95ed-f41c7f322ad0") as IntegerSlider;
            //var codeblock = GetNode("d7e88a85-d32f-416c-b449-b22f099c5471") as CodeBlockNodeModel;

            //Assert.IsNotNull(number);
            //Assert.IsNotNull(slider);
            //Assert.IsNotNull(codeblock);

            //Assert.AreEqual("10", number.Value);
            //Assert.AreEqual(0, slider.Min, 0.000001);
            //Assert.AreEqual(70, slider.Value, 0.000001);
            //Assert.AreEqual(100, slider.Max, 0.000001);

            //Assert.AreEqual("a+b;", codeblock.Code);
            //Assert.AreEqual(2, codeblock.InPorts.Count);
            //Assert.AreEqual(1, codeblock.OutPorts.Count);

            //AssertPreviewValue("d7e88a85-d32f-416c-b449-b22f099c5471", 80);

            Assert.Inconclusive("Porting : DoubleInput");
        }

        [Test, Category("Failing")]
        public void TestVerifyRuntimeValues()
        {
            RunCommandsFromFile("VerifyRuntimeValues.xml", true);
            Assert.AreEqual(2, workspace.Connectors.Count);
            Assert.AreEqual(3, workspace.Nodes.Count);

            AssertPreviewValue("9182323d-a4fd-40eb-905b-8ec415d17926", 69.12);
        }

        #endregion
    }

}
