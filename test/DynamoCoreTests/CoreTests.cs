using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests.Loggings;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class CoreTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void CanOpenGoodFile()
        {
            string openPath = Path.Combine(TestDirectory, @"core\multiplicationAndAdd\multiplicationAndAdd.dyn");
            OpenModel(openPath);

            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddANodeByName()
        {
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 1);
        }

        [Test]
        [Category("UnitTests")]
        public void CanReadPythonTemplateSetting()
        {
            // load the settings from XML file to DynamoModel
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            var settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-invalidPaths.xml");
            var pyFile = @"C:\this_folder_doesn't_exist\PythonTemplate.py";

            // check files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));

            // load the settings from file
            var settings = PreferenceSettings.Load(settingsFilePath);

            // check settings were read correctly
            Assert.IsFalse(settings == null);
            Assert.IsFalse(File.Exists(settings.PythonTemplateFilePath));
            Assert.IsFalse(File.Exists(pyFile));
            Assert.AreEqual(settings.PythonTemplateFilePath, pyFile);
        }

        [Test]
        [Category("UnitTests")]
        public void CanUpdatePythonTemplateSettings()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");
            string changedSettingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-changed.xml");
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");
            string changedPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-changed.py");
            string initialPyVerificationText = "# Unit tests Python template example";
            string updatedPyVerificationText = "# Changed Unit tests Python template example";
            string pyTemplate = "";
            string updatedPyTemplate = "";

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));
            Assert.IsTrue(File.Exists(initialPyFilePath));
            Assert.IsTrue(File.Exists(changedPyFilePath));

            // load initial settings
            // Keep in mind the initial settings file for this test has only specified a filename, not a full path
            var settings = PreferenceSettings.Load(settingsFilePath);
            settings.PythonTemplateFilePath = Path.Combine(settingDirectory, settings.PythonTemplateFilePath);

            // Assert path in settings file and in test match
            Assert.AreEqual(settings.PythonTemplateFilePath, initialPyFilePath);

            // Propagate Python template specified in settings file to DynamoModel & read it from *.py file
            CurrentDynamoModel.PreferenceSettings.PythonTemplateFilePath = settings.PythonTemplateFilePath;
            pyTemplate = File.ReadAllText(CurrentDynamoModel.PreferenceSettings.PythonTemplateFilePath);

            // Assert the template actually reads and is not empty
            Assert.IsNotEmpty(pyTemplate);
            Assert.IsTrue(pyTemplate.StartsWith(initialPyVerificationText));

            // Assert the workspace has no nodes
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 0);

            // create a Python nodeModel & add node to workspace 
            var firstPyNodeModel = new PythonNodeModels.PythonNode();
            CurrentDynamoModel.ExecuteCommand(new DynCmd.CreateNodeCommand(firstPyNodeModel, 0, 0, true, false));
            var firstPyNode = CurrentDynamoModel.CurrentWorkspace.Nodes.Last() as PythonNodeModels.PythonNode;

            // Assert a new node has been added to workspace
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 1);

            // Assert both the new python nodeModel and workspace node have been 
            // initialised with the initial Python template
            Assert.AreEqual(firstPyNodeModel.Script, pyTemplate);
            Assert.AreEqual(firstPyNode.Script, pyTemplate);
            Assert.IsTrue(firstPyNodeModel.Script.StartsWith(initialPyVerificationText));
            Assert.IsTrue(firstPyNode.Script.StartsWith(initialPyVerificationText));

            // change Python template & save settings to XML file
            CurrentDynamoModel.PreferenceSettings.PythonTemplateFilePath = changedPyFilePath;
            CurrentDynamoModel.PreferenceSettings.Save(changedSettingsFilePath);

            // load updated settings
            // no need for combining paths here as we have already done so before saving
            settings = PreferenceSettings.Load(changedSettingsFilePath);

            // update the DynamoModel settings
            CurrentDynamoModel.PreferenceSettings.PythonTemplateFilePath = settings.PythonTemplateFilePath;
            updatedPyTemplate = File.ReadAllText(CurrentDynamoModel.PreferenceSettings.PythonTemplateFilePath);

            // Assert the updated template is applied and not empty
            Assert.AreEqual(
                CurrentDynamoModel.PreferenceSettings.PythonTemplateFilePath,
                changedPyFilePath
                );
            Assert.IsNotEmpty(updatedPyTemplate);
            Assert.IsTrue(updatedPyTemplate.StartsWith(updatedPyVerificationText));

            // create a Python nodeModel & add node to workspace 
            var secondPyNodeModel = new PythonNodeModels.PythonNode();
            CurrentDynamoModel.ExecuteCommand(new DynCmd.CreateNodeCommand(secondPyNodeModel, 100, 100, true, false));
            var secondPyNode = CurrentDynamoModel.CurrentWorkspace.Nodes.Last() as PythonNodeModels.PythonNode;

            // Assert a new node has been added to workspace
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 2);

            // Assert both the new python nodeModel and workspace node have been 
            // initialised with the updated Python template
            Assert.AreEqual(secondPyNodeModel.Script, updatedPyTemplate);
            Assert.AreEqual(secondPyNode.Script, updatedPyTemplate);
            Assert.IsTrue(secondPyNodeModel.Script.StartsWith(updatedPyVerificationText));
            Assert.IsTrue(secondPyNode.Script.StartsWith(updatedPyVerificationText));

        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceModelHasCorrectDependencies()
        {
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var ws = this.CurrentDynamoModel.CustomNodeManager.CreateCustomNode("someNode", "someCategory", "");
            var csid = (ws as CustomNodeWorkspaceModel).CustomNodeId;
            var customNode = this.CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(csid);

            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Dependencies.ToList().Count());

            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Dependencies.ToList().Count());
            //assert that we still only record one dep even though custom node is in graph twice.
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Dependencies.ToList().Count());

            //assert that guid we have stored is is the custom nodes functionID
            Assert.AreEqual(customNode.FunctionSignature, CurrentDynamoModel.CurrentWorkspace.Dependencies.First());

        }

        [Test]
        [Category("UnitTests")]
        public void CanAddANote()
        {
            // Create some test note data
            Guid id = Guid.NewGuid();
            CurrentDynamoModel.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Notes.Count(), 1);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddToSelectionAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                Assert.DoesNotThrow(() => CurrentDynamoModel.AddToSelection(null));
                Assert.DoesNotThrow(() => CurrentDynamoModel.AddToSelection(5));
                Assert.DoesNotThrow(() => CurrentDynamoModel.AddToSelection("noodle"));
                Assert.DoesNotThrow(() => CurrentDynamoModel.AddToSelection(new StringBuilder()));
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddToSelectionCommand()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(addNode);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }
        }

        // Log

        [Test]
        [Category("UnitTests")]
        public void CanClearLog()
        {
            // Get the dynamo logger in non-test mode, as we write the logs
            // to dynamo console and to a file only in non-test mode. 

            DynamoLoggerTest dynamoLoggerTest = new DynamoLoggerTest();
            var logger = dynamoLoggerTest.GetDynamoLoggerWithTestModeFalse();

            Assert.AreNotEqual(0, logger.LogText.Length);

            logger.ClearLog();

            Assert.AreEqual(0, logger.LogText.Length);
        }

        // Clearworkspace

        [Test]
        [Category("UnitTests")]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            CurrentDynamoModel.ClearCurrentWorkspace();
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanClearWorkspaceWithNodes()
        {
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(new DoubleInput(), false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(new DoubleInput(), false);
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            CurrentDynamoModel.ClearCurrentWorkspace();
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests"), Category("Slow")]
        public void CanAdd100NodesToClipboard()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.Last());
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            CurrentDynamoModel.Copy();
            Assert.AreEqual(numNodes, CurrentDynamoModel.ClipBoard.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void ValidateConnectionsDoesNotClearError()
        {
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(
                new CodeBlockNodeModel("30", 100.0, 100.0, CurrentDynamoModel.LibraryServices, CurrentDynamoModel.CurrentWorkspace.ElementResolver),
                false);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // Make sure we have the number node created in active state.
            var codeBlockNode = CurrentDynamoModel.CurrentWorkspace.Nodes.First() as CodeBlockNodeModel;
            Assert.IsNotNull(codeBlockNode);
            Assert.AreEqual(ElementState.Active, codeBlockNode.State);

            // Entering an invalid value will cause it to be erroneous.
            var elementResolver = CurrentDynamoModel.CurrentWorkspace.ElementResolver;
            codeBlockNode.SetCodeContent("--", elementResolver); // Invalid numeric value.
            Assert.AreEqual(ElementState.Error, codeBlockNode.State);
            Assert.IsNotEmpty(codeBlockNode.ToolTipText); // Error tooltip text.

            // Ensure the number node is not selected now.
            Assert.AreEqual(false, codeBlockNode.IsSelected);

            // Try to select the node and make sure it is still erroneous.
            CurrentDynamoModel.AddToSelection(codeBlockNode);
            Assert.AreEqual(true, codeBlockNode.IsSelected);
            Assert.AreEqual(ElementState.Error, codeBlockNode.State);
            Assert.IsNotEmpty(codeBlockNode.ToolTipText); // Error tooltip text.

            // Deselect the node and ensure its error state isn't cleared.
            DynamoSelection.Instance.Selection.Remove(codeBlockNode);
            Assert.AreEqual(false, codeBlockNode.IsSelected);
            Assert.AreEqual(ElementState.Error, codeBlockNode.State);
            Assert.IsNotEmpty(codeBlockNode.ToolTipText); // Error tooltip text.

            // Update to valid numeric value, should cause the node to be active.
            codeBlockNode.SetCodeContent("1234;", elementResolver);
            Assert.AreEqual(ElementState.Active, codeBlockNode.State);
            Assert.IsEmpty(codeBlockNode.ToolTipText); // Error tooltip is gone.
        }

        [Test]
        [Category("UnitTests")]
        public void CanAdd1NodeToClipboardAndPaste()
        {
            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.Last());
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            CurrentDynamoModel.Copy();
            Assert.AreEqual(numNodes, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(numNodes * 2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests"), Category("Slow")]
        public void CanAdd100NodesToClipboardAndPaste()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(addNode);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            CurrentDynamoModel.Copy();
            Assert.AreEqual(numNodes, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(numNodes * 2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanCopydAndPasteNodeWithRightOffset()
        {
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            addNode.Height = 2;
            addNode.Width = 2;
            addNode.CenterX = 3;
            addNode.CenterY = 2;

            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            CurrentDynamoModel.AddToSelection(addNode);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            CurrentDynamoModel.Copy();
            Assert.AreEqual(1, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            Assert.AreEqual(22, CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(1).X);
            Assert.AreEqual(21, CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(1).Y);
        }

        [Test]
        [Category("UnitTests")]
        public void CanCopydAndPaste2NodesWithRightOffset()
        {
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            addNode.Height = 2;
            addNode.Width = 2;
            addNode.CenterX = 3;
            addNode.CenterY = 2;

            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            CurrentDynamoModel.AddToSelection(addNode);

            addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            addNode.Height = 2;
            addNode.Width = 2;
            addNode.CenterX = 6;
            addNode.CenterY = 8;

            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            CurrentDynamoModel.AddToSelection(addNode);

            CurrentDynamoModel.Copy();
            Assert.AreEqual(2, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            Assert.AreEqual(22, CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(2).X);
            Assert.AreEqual(21, CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(2).Y);

            Assert.AreEqual(25, CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(3).X);
            Assert.AreEqual(27, CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(3).Y);
        }

        [Test]
        [Category("UnitTests"), Category("Slow")]
        public void CanAdd100NodesToClipboardAndPaste3Times()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.Last());

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            CurrentDynamoModel.Copy();

            Assert.AreEqual(numNodes, CurrentDynamoModel.ClipBoard.Count);

            int numPastes = 3;
            for (int i = 1; i <= numPastes; i++)
            {
                CurrentDynamoModel.Paste();
                Assert.AreEqual(numNodes, CurrentDynamoModel.ClipBoard.Count);
                Assert.AreEqual(numNodes * (i + 1), CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddOneNodeToClipboard()
        {
            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.Last());
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            CurrentDynamoModel.Copy();
            Assert.AreEqual(numNodes, CurrentDynamoModel.ClipBoard.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanCopyAndPasteDSVarArgFunctionNode()
        {
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            const string dsVarArgFunctionName = "DSCore.String.Split@string,string[]";
            var node = new DSVarArgFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor(dsVarArgFunctionName));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            // Here we check to see if we do get a DSVarArgFunction node (which
            // is what this test case is written for, other nodes will render the
            // test case meaningless).
            //
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            CurrentDynamoModel.AddToSelection(node); // Select the only DSVarArgFunction node.
            CurrentDynamoModel.Copy(); // Copy the only DSVarArgFunction node.

            Assert.DoesNotThrow(() =>
            {
                CurrentDynamoModel.Paste(); // Nope, paste should not crash Dynamo.
            });
        }

        /// <summary>
        ///     Pasting an Input or Output node into the Home Workspace converts them
        ///     to a Code Block node.
        /// </summary>
        [Test]
        public void PasteInputAndOutputNodeInHomeWorkspace()
        {
            const string name = "Custom Node Creation Test";
            const string description = "Description";
            const string category = "Custom Node Category";

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateCustomNodeCommand(
                    Guid.NewGuid(),
                    name,
                    category,
                    description,
                    true));

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
                new Symbol(),
                0, 0,
                true, true));

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
                new Output(),
                0, 0,
                true, true));

            foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
                CurrentDynamoModel.AddToSelection(node);

            CurrentDynamoModel.Copy();

            var home = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            Assert.NotNull(home);
            SelectTabByGuid(home.Guid);

            CurrentDynamoModel.Paste();

            var homeNodes = home.Nodes;

            Assert.AreEqual(2, homeNodes.Count());
            Assert.IsInstanceOf<CodeBlockNodeModel>(homeNodes.ElementAt(0));
            Assert.IsInstanceOf<CodeBlockNodeModel>(homeNodes.ElementAt(1));
        }

        [Test]
        public void TestFileDirtyOnLacingChange()
        {
            string openPath = Path.Combine(TestDirectory, @"core\LacingTest.dyn");
            OpenModel(openPath);

            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanRedo);

            //Assert HasUnsavedChanges is false
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.HasUnsavedChanges);

            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            //Get the first node and assert the lacing strategy
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.First();
            Assert.IsNotNull(node);
            Assert.AreEqual(LacingStrategy.Auto, node.ArgumentLacing);

            //change the lacing strategy
            CurrentDynamoModel.CurrentWorkspace.UpdateModelValue(new List<Guid> { node.GUID }, "ArgumentLacing", "Longest");
            Assert.AreEqual(LacingStrategy.Longest, node.ArgumentLacing);

            Assert.AreEqual(true, CurrentDynamoModel.CurrentWorkspace.HasUnsavedChanges);
        }

        // SaveImage

        //[Test]
        //public void CanGoHomeWhenInDifferentWorkspace()
        //{
        //    // move to different workspace
        //    // go home
        //    // need to create new function via command
        //    //TODO: loadWorkspaceFromFileCommand
        //}

        // SaveAsCommand

        [Test]
        public void CanSaveAsEmptyFile()
        {
            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);

            CurrentDynamoModel.CurrentWorkspace.Save(path);

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        [Test]
        public void CanSaveAsFileWithNodesInIt()
        {
            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            }

            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            CurrentDynamoModel.CurrentWorkspace.Save(path);

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        // SaveCommand

        [Test]
        [Category("UnitTests")]
        public void CannotSaveEmptyWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            Assert.Throws<ArgumentNullException>(() => CurrentDynamoModel.CurrentWorkspace.Save(CurrentDynamoModel.CurrentWorkspace.FileName));
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.FileName, string.Empty);
        }

        [Test]
        [Category("UnitTests")]
        public void CannotSavePopulatedWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            }

            Assert.Throws<ArgumentNullException>(() => CurrentDynamoModel.CurrentWorkspace.Save(CurrentDynamoModel.CurrentWorkspace.FileName));
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.FileName, string.Empty);
        }

        [Test]
        [Category("UnitTests")]
        public void CanSelectAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                CurrentDynamoModel.OnRequestSelect(this, new ModelEventArgs(null));
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CanStayHomeWhenInHomeWorkspace()
        {
            var home = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            for (int i = 0; i < 20; i++)
            {
                CurrentDynamoModel.CurrentWorkspace = home;
                Assert.AreEqual(true, CurrentDynamoModel.CurrentWorkspace == home);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordModelsForModificationWithEmptyInput()
        {
            WorkspaceModel workspace = CurrentDynamoModel.CurrentWorkspace;
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a null argument.
            WorkspaceModel.RecordModelsForModification(null, workspace.UndoRecorder);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with an empty list.
            List<ModelBase> models = new List<ModelBase>();
            WorkspaceModel.RecordModelsForModification(models, workspace.UndoRecorder);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a list full of null.
            models.Add(null);
            models.Add(null);
            WorkspaceModel.RecordModelsForModification(models, workspace.UndoRecorder);
            Assert.AreEqual(false, workspace.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordCreatedModelsWithEmptyInput()
        {
            WorkspaceModel workspace = CurrentDynamoModel.CurrentWorkspace;
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a null argument.
            workspace.RecordCreatedModels(null);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with an empty list.
            List<ModelBase> models = new List<ModelBase>();
            workspace.RecordCreatedModels(models);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a list full of null.
            models.Add(null);
            models.Add(null);
            workspace.RecordCreatedModels(models);
            Assert.AreEqual(false, workspace.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordAndDeleteModelsWithEmptyInput()
        {
            WorkspaceModel workspace = CurrentDynamoModel.CurrentWorkspace;
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a null argument.
            workspace.RecordAndDeleteModels(null);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with an empty list.
            List<ModelBase> models = new List<ModelBase>();
            workspace.RecordAndDeleteModels(models);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a list full of null.
            models.Add(null);
            models.Add(null);
            workspace.RecordAndDeleteModels(models);
            Assert.AreEqual(false, workspace.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void CanSumTwoNumbers()
        {
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(new CodeBlockNodeModel("2", 100.0, 100.0, CurrentDynamoModel.LibraryServices, CurrentDynamoModel.CurrentWorkspace.ElementResolver), false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(new CodeBlockNodeModel("2", 100.0, 100.0, CurrentDynamoModel.LibraryServices, CurrentDynamoModel.CurrentWorkspace.ElementResolver), false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(new Watch { X = 100, Y = 300 }, false);

            ConnectorModel.Make(CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(1), CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(0), 0, 0);
            ConnectorModel.Make(CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(2), CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(0), 0, 1);
            ConnectorModel.Make(CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(0), CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(3), 0, 0);

            BeginRun();

            Thread.Sleep(250);

            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(3) is Watch, true);

            var w = (Watch)CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(3);
            Assert.AreEqual(4.0, w.CachedValue);
        }

        [Test]
        public void CanOpenDSVarArgFunctionFile()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsfunction\dsvarargfunction.dyn");
            OpenModel(openPath);

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSVarArgFunction>(
                Guid.Parse("a182d3f8-bb7d-4480-8aa5-eaacd6161415"));

            Assert.IsNotNull(node);
            Assert.IsNotNull(node.Controller.Definition);
            Assert.AreEqual(3, node.InPorts.Count);
        }

        [Test]
        public void CanOpenDoubleInputFile()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\number\TestNumberDeserialization.dyn");
            OpenModel(openPath);

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>(
                Guid.Parse("6fc905f8533f433a90fe4b9181463d53"));
            Assert.IsNotNull(node1);
            Assert.AreEqual(node1.Value, "1");

            var node2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>(
                Guid.Parse("d40969e40f3449dfb7eb20317ff91752"));
            Assert.IsNotNull(node2);
            Assert.AreEqual(node2.Value, "1");

            var node3 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>(
                Guid.Parse("8d298e3f574a420b8c789829951da295"));
            Assert.IsNotNull(node3);
            Assert.AreEqual(node3.Value, "1.1");
        }

        [Test]
        [Category("UnitTests")]
        public void SelectionDoesNotChangeWhenAddingAlreadySelectedNode()
        {
            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
                CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
                Assert.AreEqual(i + 1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

                CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.Last());
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            // the number selected stays the same
            for (int i = 0; i < numNodes; i++)
            {
                CurrentDynamoModel.AddToSelection(CurrentDynamoModel.CurrentWorkspace.Nodes.Last());
                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
            }
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_3166()
        {
            // Create the node with given information.
            NodeModel node =
                new DSVarArgFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("DSCore.List.Join@var[]..[]"));
            CurrentDynamoModel.ExecuteCommand(new DynCmd.CreateNodeCommand(node, 0, 0, true, false));

            var nodeGuid = node.GUID;

            // The node sound be found, and it should be a DSVarArgFunction.
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            Assert.IsNotNull(node);

            // Delete the node and ensure it is gone.
            CurrentDynamoModel.ExecuteCommand(new DynCmd.DeleteModelCommand(nodeGuid));
            node = workspace.NodeFromWorkspace(nodeGuid);
            Assert.IsNull(node);

            // Perform undo operation.
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            CurrentDynamoModel.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            // Now that deletion is undone, ensure the node exists.
            node = workspace.NodeFromWorkspace(nodeGuid);
            Assert.IsNotNull(node);
            Assert.IsNotNull(node as DSVarArgFunction);
        }

        [Test]
        [Category("UnitTests")]
        public void ValidateWrongConnectionIsNotCreated()
        {
            //MAGN-7334 Add test case for checking wrong connector is not created

            var numberGuid = Guid.NewGuid().ToString();
            var numberName = "Number";
            var pointGuid = Guid.NewGuid().ToString();
            var pointName = "List.Create";
            var unexistingNodeGuid = Guid.NewGuid().ToString();

            var commands = new List<DynamoModel.ModelBasedRecordableCommand>
            {
                new DynamoModel.CreateNodeCommand(numberGuid,
                    numberName, 0, 0, false, false),
                new DynamoModel.CreateNodeCommand(pointGuid,
                    pointName, 0, 0, false, false),
                new DynamoModel.ModelEventCommand(pointGuid, "AddInPort"),

                new DynamoModel.MakeConnectionCommand(numberGuid, 0, PortType.Output,
                    DynamoModel.MakeConnectionCommand.Mode.Begin),
                new DynamoModel.MakeConnectionCommand(unexistingNodeGuid, 0,
                    PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End),

                new DynamoModel.MakeConnectionCommand(unexistingNodeGuid, 0,
                    PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin),
                new DynamoModel.MakeConnectionCommand(pointGuid, 0, PortType.Input,
                    DynamoModel.MakeConnectionCommand.Mode.End)
            };

            commands.ForEach(c =>
            {
                try
                {
                    CurrentDynamoModel.ExecuteCommand(c);
                }
                catch
                {
                    // Make sure that only MakeConnectionCommand throws an exception
                    Assert.IsInstanceOf<DynamoModel.MakeConnectionCommand>(c);
                }
            });

            Assert.IsFalse(CurrentDynamoModel.CurrentWorkspace.Connectors.Any());
        }

        private void CreateNodeAndPorts(int count)
        {
            var pointGuid = Guid.NewGuid().ToString();
            const string pointName = "List.Create";

            var commands = new List<DynamoModel.ModelBasedRecordableCommand>
            {
                new DynamoModel.CreateNodeCommand(pointGuid,
                    pointName, 0, 0, false, false),
                new DynamoModel.ModelEventCommand(pointGuid, "SetInPortCount", count),
            };

            commands.ForEach(c => { CurrentDynamoModel.ExecuteCommand(c); });
        }

        [Test]
        [Category("UnitTests")]
        public void ModelEventCommand_SetInPortCount_nPortsRequested_nPortsCreated()
        {
            CreateNodeAndPorts(2);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            Assert.NotNull(node);
            Assert.AreEqual(node.InPorts.Count, 2);
        }

        [Test]
        [Category("UnitTests")]
        public void ModelEventCommand_SetInPortCount_Zero_LeavesOnePort()
        {
            CreateNodeAndPorts(0);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            Assert.NotNull(node);
            Assert.AreEqual(node.InPorts.Count, 1);
        }

        [Test]
        public void UpstreamNodesComputedCorrectly()
        {
            string openPath = Path.Combine(TestDirectory, @"core\transpose.dyn");
            //this should compute all upstream nodes on each node from the roots down
            OpenModel(openPath);

            //this asserts that each node's UpstreamCache contains the same list as the recursively computed AllUpstreamNodes
            foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
            {
                Assert.IsTrue(node.UpstreamCache.SetEquals(node.AllUpstreamNodes(new List<NodeModel>())));
            }
        }

        [Test]
        [Category("UnitTests")]
        public void UpdatingAWorkspaceWithExtraViewInfo_DoesNotDupliateNotesAndGroups()
        {
            //add a node to the currentWorkspace
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode);
            //put the node in a group
            DynamoSelection.Instance.Selection.Add(addNode);
            //create the group around selected node
            Guid groupid = Guid.NewGuid();
            var annotation = this.CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupid);

            Assert.AreEqual(this.CurrentDynamoModel.CurrentWorkspace.Annotations.Count(), 1);

            //add a note the current workspace
            var newNote = new NoteModel(100, 100, "someText", Guid.NewGuid());
            this.CurrentDynamoModel.CurrentWorkspace.AddNote(newNote, false);

            //now call update with some test data

            var mockViewBlock = new ExtraWorkspaceViewInfo();
            mockViewBlock.Annotations = new[] {
                new ExtraAnnotationViewInfo()
                { Id =groupid.ToString(),
                    Title = annotation.AnnotationText,
                    Nodes =annotation.Nodes.Select(x=>x.GUID.ToString()).ToList(),
                    FontSize = annotation.FontSize,
                    Background = annotation.Background,
                    Left = annotation.X,
                    Top = annotation.Y,
                },

                new ExtraAnnotationViewInfo()
                { Id =newNote.GUID.ToString(),
                    Title = newNote.Text,
                    Nodes = new List<string>(),
                    Left = annotation.X,
                    Top = annotation.Y,
                }
            };


            this.CurrentDynamoModel.CurrentWorkspace.UpdateWithExtraWorkspaceViewInfo(mockViewBlock);
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Annotations.Count());
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());
        }
    }
}