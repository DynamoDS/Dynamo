using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Dynamo.Wpf.ViewModels;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class WorkspaceSaving : DynamoViewModelUnitTest
    {

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("Builtin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceCanSaveAsNewFile()
        {
            // new home workspace
            // save as
            // file exists  

            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull( dynamoModel.CurrentWorkspace );
            Assert.IsAssignableFrom( typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace );

            var newPath = GetNewFileNameOnTempPath("dyn");
            dynamoModel.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        [Category("UnitTests")]
        public void CleanWorkbenchClearsUndoStack()
        {
            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);

            var workspace = dynamoModel.CurrentWorkspace;
            Assert.AreEqual(false, workspace.CanUndo);
            Assert.AreEqual(false, workspace.CanRedo);
            Assert.AreEqual(0, workspace.Nodes.Count()); // An empty workspace

            var addNode = new DSFunction(dynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var createNodeCommand = new DynamoModel.CreateNodeCommand(
                addNode, 0, 0, false, false);

            // Create a new node in the empty workspace.
            ViewModel.ExecuteCommand(createNodeCommand);
            Assert.AreEqual(1, workspace.Nodes.Count());

            Assert.AreEqual(true, workspace.CanUndo);
            Assert.AreEqual(false, workspace.CanRedo);
            dynamoModel.CurrentWorkspace.Clear(); // Clearing current workspace.

            // Undo stack should be cleared.
            Assert.AreEqual(false, workspace.CanUndo);
            Assert.AreEqual(false, workspace.CanRedo);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceCanSaveAsNewFile()
        {
            // new custom node
            // save as
            // file exists

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var ws = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");
                
            var newPath = GetNewFileNameOnTempPath("dyf");
            dynamoModel.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));

        }

        [Test]
        public void HomeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\math", "Add.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var newPath = GetNewFileNameOnTempPath("dyn");
            model.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);

            Assert.IsNotNull(nodeWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyf");
            model.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        public void HomeWorkspaceCanSaveAsFailsWithoutFilepathArg()
        {
            // open file
            // save as
            // file exists
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\math", "Add.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            Assert.Throws<ArgumentNullException>(()=> model.CurrentWorkspace.Save(""));
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceCanSaveAsFailsWithoutFilepathArg()
        {
            // open file
            // save as
            // file exists

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");

            Assert.Throws<ArgumentNullException>(()=>def.Save(null));
        }

        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceSaveFailsWithoutFilepathProperty()
        {
            // new file
            // save fails

            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            Assert.Throws<ArgumentNullException>(()=>ViewModel.Model.CurrentWorkspace.Save(ViewModel.Model.CurrentWorkspace.FileName));
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspacSaveFailsWithoutFilepathProperty()
        {
            // new file
            // save fails

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");

            Assert.Throws<ArgumentNullException>(()=>def.Save(def.FileName));
        }

        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceFilePathPropertyIsUpdatedOnSaveAs()
        {
            // open file
            // save as
            // FileName is updated

            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyn");
            dynamoModel.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceFilePathIsUpdatedOnSaveAs()
        {
            // open file
            // save as
            // FileName is updated

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);

            var newPath = GetNewFileNameOnTempPath("dyf");
            def.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));

            Assert.AreEqual(newPath, def.FileName );
        }

        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceCanSaveAsMultipleTimes()
        {
            // open file
            // for x 10
            // save as
            // file exists, filepath updated

            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            foreach (var i in Enumerable.Range(0, 10))
            {
                var newPath = GetNewFileNameOnTempPath("dyn");
                dynamoModel.CurrentWorkspace.Save(newPath);

                Assert.IsTrue(File.Exists(newPath));
                Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceCanSaveAsMultipleTimes()
        {
            // open file
            // for x 10
            // save as
            // file exists, filepath updated 

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);

            foreach (var i in Enumerable.Range(0, 10))
            {
                var newPath = GetNewFileNameOnTempPath("dyf");
                def.Save(newPath);

                Assert.IsTrue(File.Exists(newPath));

                Assert.AreEqual(newPath, def.FileName);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceCanSaveAsAndThenSaveExistingFile()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save
            // file is updated

            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyn");
            dynamoModel.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            Thread.Sleep(1);
            dynamoModel.CurrentWorkspace.Save(dynamoModel.CurrentWorkspace.FileName);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var saveTime = File.GetLastWriteTime(newPath);

            // assert the file has new update
            Assert.Greater(saveTime.Ticks - saveAsTime.Ticks, 0);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceCanSaveAsAndThenSaveExistingFile()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save
            // file is updated

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);

            var newPath = GetNewFileNameOnTempPath("dyf");
            def.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            Thread.Sleep(1);
            def.Save(def.FileName);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.FileName);
            var saveTime = File.GetLastWriteTime(newPath);

            // assert the file has new update
            Assert.Greater(saveTime.Ticks - saveAsTime.Ticks, 0);
        }

        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceCanSaveMultipleTimes()
        {
            // open file
            // save
            // file is updated
            // save
            // file is updated

            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyn");
            dynamoModel.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                dynamoModel.CurrentWorkspace.Save(dynamoModel.CurrentWorkspace.FileName);

                Assert.IsTrue(File.Exists(newPath));
                Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
                var saveTime = File.GetLastWriteTime(newPath);

                // assert the file has new update
                Assert.Greater(saveTime.Ticks - lastSaveTime.Ticks, 0);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceCanSaveMultipleTimes()
        {
            // open file
            // save
            // file is updated
            // save
            // file is updated

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);

            var newPath = GetNewFileNameOnTempPath("dyf");
            def.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                def.Save(def.FileName);

                Assert.IsTrue(File.Exists(newPath));
                Assert.AreEqual(newPath, def.FileName);
                var saveTime = File.GetLastWriteTime(newPath);

                // assert the file has new update
                Assert.Greater(saveTime.Ticks - lastSaveTime.Ticks, 0);
                lastSaveTime = saveTime;
            }
        }

        [Test]
        [Category("UnitTests")]
        public void EnsureSaveDialogIsShownOnOpenIfSaveCommand()
        {
            //workspace has unsaved changes
            this.GetModel().CurrentWorkspace.HasUnsavedChanges = true;
            //openPath
            string openPath = Path.Combine(TestDirectory, (@"UI\GroupTest.dyn"));
            var eventCount = 0;

            var handler = new WorkspaceSaveEventHandler((o,e) => { eventCount = eventCount + 1; });
            //attach handler to the save request
            ViewModel.RequestUserSaveWorkflow += handler;
            //send the command
            ViewModel.OpenIfSavedCommand.Execute(new Dynamo.Models.DynamoModel.OpenFileCommand(openPath));

            //dispose handler
            ViewModel.RequestUserSaveWorkflow -= handler;

            //assert the request was made
            Assert.AreEqual(1,eventCount);

            //assert the filePath was upated correctly
            //use reflection to get a private field: 
            //TODO(mk) write a better test without reflection or internal which does not raise save dialog in test mode.
            var filePath = ViewModel.GetType().GetField("filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(ViewModel);
            Assert.AreEqual(openPath,filePath );
        }

        [Test]
        [Category("UnitTests")]
        public void EnsureAskUserToSaveDialogIsShownOnOpenRecent()
        {
            //openPath
            string openPath = Path.Combine(TestDirectory, (@"UI\GroupTest.dyn"));

            //workspace has unsaved changes
            this.GetModel().CurrentWorkspace.HasUnsavedChanges = true;

            var eventCount = 0;

            var handler = new WorkspaceSaveEventHandler((o, e) => { eventCount = eventCount + 1; });

            //attach handler to the save request
            ViewModel.RequestUserSaveWorkflow += handler;

            //send the command
            ViewModel.OpenRecentCommand.Execute(openPath);

            //dispose handler
            ViewModel.RequestUserSaveWorkflow -= handler;

            //assert the request was made
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        [Category("UnitTests")]
        public void EnsureOnOpenIfSaveCommandOpensDynFile()
        {
            
            //openPath
            string openPath = Path.Combine(TestDirectory, (@"UI\GroupTest.dyn"));
            //send the command
            ViewModel.OpenIfSavedCommand.Execute(new Dynamo.Models.DynamoModel.OpenFileCommand(openPath));

            Assert.GreaterOrEqual(2, GetModel().CurrentWorkspace.Nodes.ToList().Count());

        }


        [Test]
        [Category("UnitTests")]
        public void HomeWorkspaceHasUnsavedChangesPropertyIsSetOnSaveAs()
        {
            // open file
            // make change
            // saveAs
            // SavedProperty is true, filePath set, file exists

            // get empty workspace
            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            // make change
            var node = new DSFunction(dynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            dynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            // save
            var newPath = GetNewFileNameOnTempPath("dyn");
            dynamoModel.CurrentWorkspace.Save(newPath);

            // check expected
            Assert.IsFalse(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);

        }

        // TODO: Enable when Open() is expanded to open Json
        [Test]
        [Category("UnitTests")]
        public void CanSaveAndReadWorkspaceDescription()
        {
            // get empty workspace
            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);

            // set description
            dynamoModel.CurrentWorkspace.Description = "dummy description";

            // save
            var newPath = GetNewFileNameOnTempPath("dyn");
            dynamoModel.CurrentWorkspace.Save(newPath);

            // load
            ViewModel.Model.OpenFileFromPath(newPath);
            Assert.AreEqual("dummy description", ViewModel.Model.CurrentWorkspace.Description);
        }

        [Test]
        [Category("UnitTests")]
        public void CanSaveAndReadWorkspaceName()
        {
            // get empty workspace
            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.AreEqual("Home", ViewModel.Model.CurrentWorkspace.Name);

            // get file path and name of file
            var filePath = GetNewFileNameOnTempPath("dyn");
            var fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);
            if (extension == ".dyn" || extension == ".dyf")
            {
              fileName = Path.GetFileNameWithoutExtension(filePath);
            }

            // save
            ViewModel.SaveAs(filePath);

            // load
            ViewModel.Model.OpenFileFromPath(filePath);
            Assert.AreEqual(fileName, ViewModel.Model.CurrentWorkspace.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void CanSaveDifferentTargetWorkspace()
        {
            // get empty workspace
            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.AreEqual("Home", ViewModel.Model.CurrentWorkspace.Name);

            // get file path and name of file
            var filePath = GetNewFileNameOnTempPath("dyn");
            var fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);
            if (extension == ".dyn" || extension == ".dyf")
            {
                fileName = Path.GetFileNameWithoutExtension(filePath);
            }

            // Open a dyf which making the current workspace become custom node workspace
            var originalDyf = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.Model.OpenFileFromPath(originalDyf);

            // save Home workspace
            ViewModel.SaveAs(ViewModel.Workspaces[0].Model.Guid, filePath);

            // save custom node workspace
            var dyfFilePath = GetNewFileNameOnTempPath("dyf");
            ViewModel.SaveAs(ViewModel.Workspaces[1].Model.Guid, dyfFilePath);

            // Open the serialized files to verify
            ViewModel.Model.OpenFileFromPath(filePath);
            // Home workspace name should be file name
            Assert.AreEqual(fileName, ViewModel.Model.CurrentWorkspace.Name);

            ViewModel.Model.OpenFileFromPath(dyfFilePath);
            Assert.AreEqual("Constant2", ViewModel.Model.CurrentWorkspace.Name);
        }

        [Test]
        public void CanSaveAsNewWorkspaceWithNewGuids()
        {
            string examplePath = Path.Combine(TestDirectory, (@"UI\GroupStyleTest.dyn"));
            ViewModel.OpenCommand.Execute(examplePath);
            var legacyWorkspaceGuid = ViewModel.Model.CurrentWorkspace.Guid;
            var legacyNodeGuid = ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault().GUID;
            var legacyGroupGuid = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault().AnnotationModel.GUID;
            var legacyGroupStyleId = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault().GroupStyleId;
            var legacyNoteId = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault().GUID;
            var legacyConnectorId = ViewModel.Model.CurrentWorkspace.Connectors.FirstOrDefault().GUID;
            var legacyLinterId = ViewModel.CurrentSpaceViewModel.Model.linterManager.ActiveLinter.Id;

            // Save As a new workspace
            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            ViewModel.CurrentSpaceViewModel.Save(path, false, null, SaveContext.SaveAs);
            ViewModel.OpenCommand.Execute(path);
            var newWorkspaceGuid = ViewModel.Model.CurrentWorkspace.Guid;
            var newNodeGuid = ViewModel.Model.CurrentWorkspace.Nodes.FirstOrDefault().GUID;
            var newGroupGuid = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault().AnnotationModel.GUID;
            var newGroupStyleId = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault().GroupStyleId;
            var newNoteId = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault().GUID;
            var newConnectorId = ViewModel.Model.CurrentWorkspace.Connectors.FirstOrDefault().GUID;
            var newLinterId = ViewModel.CurrentSpaceViewModel.Model.linterManager.ActiveLinter.Id;

            Assert.AreNotEqual(legacyWorkspaceGuid, newWorkspaceGuid);
            Assert.AreNotEqual(legacyNodeGuid, newNodeGuid);
            Assert.AreNotEqual(legacyGroupGuid, newGroupGuid);
            Assert.AreNotEqual(legacyNoteId, newNoteId);
            Assert.AreNotEqual(legacyConnectorId, newConnectorId);
            Assert.AreEqual(legacyGroupStyleId, newGroupStyleId);
            Assert.AreEqual(legacyLinterId, newLinterId);
        }

        [Test]
        [Category("UnitTests")]
        public void BackUpSaveDoesNotChangeName()
        {
            // get empty workspace
            var dynamoModel = ViewModel.Model;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.AreEqual("Home", ViewModel.Model.CurrentWorkspace.Name);

            // get file path and name of file
            var filePath = GetNewFileNameOnTempPath("dyn");
            var fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);
            if (extension == ".dyn" || extension == ".dyf")
            {
                fileName = Path.GetFileNameWithoutExtension(filePath);
            }

            // save
            ViewModel.SaveAs(filePath,true);

            // load
            ViewModel.Model.OpenFileFromPath(filePath);
            Assert.AreEqual("Home", ViewModel.Model.CurrentWorkspace.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeSaveDoesNotChangeName()
        {

            var funcguid = GuidUtility.Create(GuidUtility.UrlNamespace, "NewCustomNodeSaveAndLoad");
            //first create a new custom node.
            this.ViewModel.ExecuteCommand(new DynamoModel.CreateCustomNodeCommand(funcguid, "testnode", "testcategory", "atest", true));
            var outnode1 = new Output();
            outnode1.Symbol = "out1";
            var outnode2 = new Output();
            outnode1.Symbol = "out2";

            var cbn = new CodeBlockNodeModel(this.ViewModel.EngineController.LibraryServices);
            cbn.SetCodeContent("5;",this.ViewModel.CurrentSpace.ElementResolver);

            ViewModel.FocusCustomNodeWorkspace(funcguid);

            this.ViewModel.CurrentSpace.AddAndRegisterNode(cbn);
            this.ViewModel.CurrentSpace.AddAndRegisterNode(outnode1);
            this.ViewModel.CurrentSpace.AddAndRegisterNode(outnode2);

            new ConnectorModel(cbn.OutPorts.FirstOrDefault(), outnode1.InPorts.FirstOrDefault(), Guid.NewGuid());
            new ConnectorModel(cbn.OutPorts.FirstOrDefault(), outnode2.InPorts.FirstOrDefault(), Guid.NewGuid());

            var savePath = GetNewFileNameOnTempPath("dyf");
           this.ViewModel.SaveAs(savePath);

            //assert the filesaved
            Assert.IsTrue(File.Exists(savePath));
            Assert.IsFalse(string.IsNullOrEmpty(File.ReadAllText(savePath)));

            // get file path and name of file
            var fileName = Path.GetFileName(savePath);
            string extension = Path.GetExtension(savePath);
            if (extension == ".dyn" || extension == ".dyf")
            {
                fileName = Path.GetFileNameWithoutExtension(savePath);
            }

            Assert.IsTrue(ViewModel.CurrentSpace is CustomNodeWorkspaceModel);
            //close the customNode so we can reopen it.
            this.ViewModel.Model.RemoveWorkspace(ViewModel.CurrentSpace);

            // load
            ViewModel.OpenCommand.Execute(savePath);
            ViewModel.FocusCustomNodeWorkspace(funcguid);

            Assert.IsTrue(ViewModel.CurrentSpace is CustomNodeWorkspaceModel);
            //assert the current workspace is the customNode.
            Assert.IsTrue(File.Exists(savePath));
            Assert.AreEqual("testnode", ViewModel.Model.CurrentWorkspace.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceHasUnsavedChangesPropertyIsSetOnSaveAs()
        {
            // open file
            // make change
            // saveAs
            // SavedProperty is true, filePath set, file exists

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);
            Assert.IsFalse(def.HasUnsavedChanges);

            var node = new DSFunction(dynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            def.AddAndRegisterNode(node, false);
            Assert.IsTrue(def.HasUnsavedChanges);
            Assert.AreEqual(1, def.Nodes.Count() );
            
            var newPath = GetNewFileNameOnTempPath("dyf");
            def.Save(newPath);

            Assert.IsFalse(def.HasUnsavedChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void WorkspaceWithDummyXmlNodesSavesAndOpensWithoutThrowing()
        {
            //openPath
            var testFileWithMultipleXmlDummyNode = @"core\dummy_node\dummyNodeXMLMultiple.dyn";
            string openPath = Path.Combine(TestDirectory, testFileWithMultipleXmlDummyNode);
            ViewModel.OpenCommand.Execute(openPath);
            var nodeCount1 = ViewModel.CurrentSpace.Nodes.Count();

            //try saving this graph
            var newPath = GetNewFileNameOnTempPath("dyn");
            Assert.DoesNotThrow(()=> { this.ViewModel.SaveAs(newPath); }) ;

            //try to open the file we just saved.
            Assert.DoesNotThrow(() => { ViewModel.OpenCommand.Execute(newPath); });
            var nodeCount2 = ViewModel.CurrentSpace.Nodes.Count();
            //assert we are missing the dummy nodes after opening
            Assert.Less(nodeCount2, nodeCount1);
            Assert.IsNotNull(this.ViewModel);
            Assert.DoesNotThrow(() => { ViewModel.Model.ClearCurrentWorkspace(); });
            System.IO.File.Delete(newPath);
        }

        #region CustomNodeWorkspaceModel SaveAs side effects

        [Test]
        [Category("UnitTests")]
        public void CustomNodeSaveAsDoesGiveNewFunctionIdToNewCustomNode()
        {
            // new custom node
            // SaveAs
            // custom node instance has new function id
            // custom node instance is in environment

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";
            var initialId = Guid.NewGuid();

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", initialId);
            var workspace = (CustomNodeWorkspaceModel)def;

            var newPath = GetNewFileNameOnTempPath("dyf");
            workspace.Save(newPath);

            var newDef = workspace.CustomNodeId;

            Assert.AreNotEqual(Guid.Empty, initialId);
            Assert.AreNotEqual(Guid.Empty, newDef);
            Assert.AreEqual(initialId, newDef);

            var newPath2 = GetNewFileNameOnTempPath("dyf");
            workspace.Save(newPath2);
            var newDef2 = workspace.CustomNodeId;
            Assert.AreNotEqual(Guid.Empty, newDef2);
            Assert.AreNotEqual(initialId, newDef2);
        }

        [Test]
        public void CustomNodeSaveAsGivesNewFunctionIdToExistingFile()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // custom node instance is in environment

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = GetNewFileNameOnTempPath("dyf");
            nodeWorkspace.Save(newPath); // introduces new function id

            Assert.IsTrue(File.Exists(newPath));

            // workspace now has different function id
            var newDef = nodeWorkspace.CustomNodeDefinition;

            Assert.AreNotEqual(newDef.FunctionId, oldId);

        }

        [Test]
        public void CustomNodeSaveAsAddsNewLoadedNodeToCustomNodeManager()
        {
            // open custom node
            // SaveAs
            // new node with new function id in custom node manager with save name
            // can get instance of that node

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.OfType<CustomNodeWorkspaceModel>().FirstOrDefault();
            
            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = GetNewFileNameOnTempPath("dyf");
            nodeWorkspace.Save(newPath); // introduces new function id

            Assert.IsTrue(File.Exists(newPath));

            // workspace now has different function id
            var newDef = nodeWorkspace.CustomNodeDefinition;
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.IsInitialized(newDef.FunctionId));
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.Contains(oldId));
        }

        [Test]
        public void CustomNodeEditNodeDescriptionKeepingViewBlockInDyf()
        {
            // new custom node
            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";
            var initialId = Guid.NewGuid();

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", initialId);
            var workspace = (CustomNodeWorkspaceModel)def;

            // Set file path
            workspace.FileName = GetNewFileNameOnTempPath("dyf");
            // search common base name
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Visible = true;
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Cool");
            // results are correct
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count());

            var newCustNodeInstance = dynamoModel.CustomNodeManager.CreateCustomNodeInstance(initialId);
            dynamoModel.CurrentWorkspace.AddAndRegisterNode(newCustNodeInstance, false);

            // run expression
            Assert.AreEqual(1, dynamoModel.CurrentWorkspace.Nodes.Count());

            // Mimic UI behavior so that custom node updates node name
            FunctionNodeViewCustomization temp = new FunctionNodeViewCustomization();
            FunctionNamePromptEventArgs args = new FunctionNamePromptEventArgs
            {
                Success = true,
                Name = "Cool node_v2",
                Category = "Custom Nodes",
                CanEditName = false
            };
            temp.SerializeCustomNodeWorkspaceWithNewInfo(args, ViewModel, newCustNodeInstance);

            // Check if serialized workspace still have view block
            string fileContents = File.ReadAllText(workspace.FileName);
            var Jobject = Newtonsoft.Json.Linq.JObject.Parse(fileContents);
            Assert.IsTrue(Jobject["View"] != null);
            Assert.IsTrue(Jobject["Name"].ToString() == "Cool node_v2");
        }

        [Test]
        public void CustomNodeEditNodeDescriptionKeepingIsVisibleInDynamoLibraryInDyf()
        {
            // Open an existing dyf
            var dynamoModel = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\combine", "Sequence_Json.dyf");
            ViewModel.OpenCommand.Execute(examplePath);
            var customNodeWorkspace = dynamoModel.CurrentWorkspace;

            var initialId = new Guid("6aecda57-7679-4afb-aa02-05a75cc3433e");
            var newCustNodeInstance = dynamoModel.CustomNodeManager.CreateCustomNodeInstance(initialId, null, true);
            // Switch HomeWorkspace and place custom node on it
            dynamoModel.CurrentWorkspace = dynamoModel.Workspaces.First();
            dynamoModel.CurrentWorkspace.AddAndRegisterNode(newCustNodeInstance, false);

            // run expression
            Assert.AreEqual(1, dynamoModel.CurrentWorkspace.Nodes.Count());

            // Mimic UI behavior so that custom node updates node name
            FunctionNodeViewCustomization temp = new FunctionNodeViewCustomization();
            FunctionNamePromptEventArgs args = new FunctionNamePromptEventArgs
            {
                Success = true,
                Name = "Sequence2",
                Category = "Misc",
                CanEditName = false,
                Description = "test node"
            };
            temp.SerializeCustomNodeWorkspaceWithNewInfo(args, ViewModel, newCustNodeInstance);

            // Check if serialized workspace still have view block
            string fileContents = File.ReadAllText(customNodeWorkspace.FileName);
            var Jobject = Newtonsoft.Json.Linq.JObject.Parse(fileContents);
            Assert.IsTrue(Jobject["View"] != null);
            // Expect matching value
            Assert.IsTrue(Jobject["View"]["Dynamo"]["IsVisibleInDynamoLibrary"].ToString() == "True");
        }

        [Test]
        public void CustomNodeSaveAsAddsNewIdToEnvironmentAndMaintainsOldOne()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // function id is in environment

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = GetNewFileNameOnTempPath("dyf");
            nodeWorkspace.Save(newPath);

            var newDef = nodeWorkspace.CustomNodeDefinition;
            Assert.AreNotEqual(oldId, newDef.FunctionId);
        }

        [Test]
        public void CustomNodeSaveAsNewCustomNodeCanBeUsedInWorkspaceAndExpressionRuns()
        {
            // open custom node
            // SaveAs
            // place custom node with new id, run expression and result is correct.

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);


            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyf");
            nodeWorkspace.Save(newPath); // introduces new function id

            Assert.IsTrue(File.Exists(newPath));

            // get new function id

            // put in workspace
            model.CurrentWorkspace = ViewModel.HomeSpace;
            var newCustNodeInstance =
                model.CustomNodeManager.CreateCustomNodeInstance(nodeWorkspace.CustomNodeId);
            model.CurrentWorkspace.AddAndRegisterNode(newCustNodeInstance, false);

            // run expression
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count());

            // run expression is correct
            ViewModel.HomeSpace.Run();

            var evaluatedNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            Assert.AreEqual(2.0, evaluatedNode.CachedValue.Data);
        }

        [Test]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearch()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, difer 

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = Path.Combine(TempFolder, "Constant2.dyf");
            var originalNumElements = ViewModel.Model.SearchModel.NumElements;
            nodeWorkspace.Save(newPath); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Visible = true;
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Constant2");
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.NumElements);

            Assert.AreEqual(2, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count());

            var res1 = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.ElementAt(0);
            var res2 = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.ElementAt(1);

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res1);
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res2);

            var node1 = res1.Model as CustomNodeSearchElement;
            var node2 = res2.Model as CustomNodeSearchElement;

            Assert.IsTrue((node1.ID == oldId && node2.ID == newId) ||
                          (node1.ID == newId && node2.ID == oldId));

        }

        [Test]
        public void CustomNodeSaveAsPreservesFunctionDefinitionsOfExistingInstances()
        {
            // open custom node
            // place the custom node a few times in home workspace
            // SaveAs
            // can get instances of original custom node

            // open custom node
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeId;

            // place the custom node a few times in home workspace
            var homeWorkspace = model.Workspaces.OfType<HomeWorkspaceModel>().First();
            model.CurrentWorkspace = homeWorkspace;
            foreach (var _ in Enumerable.Range(0, 10))
            {
                var node = model.CustomNodeManager.CreateCustomNodeInstance(oldId);
                model.CurrentWorkspace.AddAndRegisterNode(node, false);
            }

            // SaveAs
            var newPath = GetNewFileNameOnTempPath("dyf");
            var newName = Path.GetFileNameWithoutExtension(newPath);
            nodeWorkspace.Save(newPath); // introduces new function id

            Assert.IsTrue(File.Exists(newPath));

            Assert.IsNotNull(nodeWorkspace.CustomNodeDefinition);

            // can get instances of original custom node
            Assert.AreEqual(10, homeWorkspace.Nodes.Count());
            var funcs =
                homeWorkspace.Nodes.OfType<Function>()
                    .Where(x => x.Definition.FunctionId == nodeWorkspace.CustomNodeId)
                    .ToList();
            Assert.AreEqual(10, funcs.Count);
            funcs.ForEach(x => Assert.AreEqual(newName, x.Name));
            
        }

        [Test]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearchAndItCanBeRefactoredWhilePreservingOriginalFromExistingDyf()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, differ 

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var oldId = (model.CurrentWorkspace as CustomNodeWorkspaceModel).CustomNodeDefinition.FunctionId;

            CustomNodeWorkspaceModel nodeWorkspace;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(oldId, true, out nodeWorkspace));
            
            var newPath = GetNewFileNameOnTempPath("dyf");
            var originalNumElements = ViewModel.Model.SearchModel.NumElements;

            // save as
            nodeWorkspace.Save(newPath); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            CustomNodeWorkspaceModel newWs;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(newId, true, out newWs));

            // refactor oldId with new name
            newWs.SetInfo("TheNoodle", "TheCat", "TheCat");

            // num elements is unchanged by refactor
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.NumElements);

            // search for refactored node
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Visible = true;
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("TheNoodle");

            // results are correct
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count());
            var node3 = (CustomNodeSearchElement)ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.ElementAt(0).Model;
            Assert.AreEqual(newId, node3.ID);

            // search for un-refactored node
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Constant2");

            // results are correct
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count());
            var node4 = (CustomNodeSearchElement)ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.ElementAt(0).Model;
            Assert.AreEqual(oldId, node4.ID);

        }

        [Test]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearchAndItCanBeRefactoredWhilePreservingOriginalFromExistingDyf2()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, differ 

            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var oldId = (model.CurrentWorkspace as CustomNodeWorkspaceModel).CustomNodeDefinition.FunctionId;


            CustomNodeWorkspaceModel nodeWorkspace;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(oldId, true, out nodeWorkspace));

            var newPath = GetNewFileNameOnTempPath("dyf");
            var originalNumElements = ViewModel.Model.SearchModel.NumElements;

            // save as
            nodeWorkspace.Save(newPath); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            // refactor oldId with new name

            CustomNodeWorkspaceModel newWs;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(newId, true, out newWs));

            // refactor oldId with new name
            newWs.SetInfo("Constant2 Alt", "TheCat", "TheCat");

            // num elements is unchanged by refactor
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.NumElements);

            // search common base name
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Visible = true;
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Constant2");

            // results are correct
            Assert.AreEqual(2, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count());

            var res1 = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.ElementAt(0);
            var res2 = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.ElementAt(1);

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res1);
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res2);

            var node1 = (CustomNodeSearchElement)res1.Model;
            var node2 = (CustomNodeSearchElement)res2.Model;

            Assert.IsTrue((node1.ID == oldId && node2.ID == newId) ||
                          (node1.ID == newId && node2.ID == oldId));
        }

        [Test]
        public void MultipleCustomNodeSaveAsOperationsAddsMultipleValidFunctionIdsToCustomNodeManager()
        {

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);
            var workspace = (CustomNodeWorkspaceModel)def;

            var listGuids = new List<Guid>();
            var listNames = new List<string>();

            for (var i = 0; i < 10; i++)
            {
                var newPath = GetNewFileNameOnTempPath("dyf");
                workspace.Save(newPath);

                var newId = workspace.CustomNodeDefinition.FunctionId;
                var newName = workspace.Name;

                listGuids.Add(newId);
                listNames.Add(newName);

                listGuids.ForEach( x => Assert.IsTrue( ViewModel.Model.CustomNodeManager.NodeInfos.ContainsKey(x) ));
                listNames.ForEach( x => Assert.IsTrue( ViewModel.Model.CustomNodeManager.Contains(x) ));
            }
        }

        [Test]
        public void CustomNodeSaveAsKeepItsConnectors()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);
            var workspace = model.CurrentWorkspace as CustomNodeWorkspaceModel;
            var connectorCount = workspace.Connectors.Count();
            var nodeCount = workspace.Nodes.Count();

            for (var i = 0; i < 10; i++)
            {
                var newName = Guid.NewGuid().ToString();
                var newPath = Path.Combine(TempFolder, Path.ChangeExtension(newName, "dyf"));
                workspace.Save(newPath);

                Assert.AreEqual(connectorCount, workspace.Connectors.Count());
                Assert.AreEqual(nodeCount, workspace.Nodes.Count());
            } 
        }

        [Test]
        public void CusotmNodeSaveAsUpdateItsName()
        {
            //
            var dynamoModel = ViewModel.Model;
            var nodeName = "Foo";
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);
            var workspace = (CustomNodeWorkspaceModel)def;
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Visible = true;

            for (var i = 0; i < 10; i++)
            {
                var newName = Guid.NewGuid().ToString();
                var newPath = Path.Combine(TempFolder, Path.ChangeExtension(newName, "dyf"));
                workspace.Save(newPath);

                // Verify the name of this workspace is the same as the file name we specify
                Assert.AreEqual(newName, workspace.Name);

                // Verify new name is searchable
                ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults(newName);
                Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count());

                // Verify search element's name is new name
                var res = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.First();
                Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res);
                Assert.AreEqual(res.Name, newName);

                // Verify the search instance use new guid
                var node = res.Model as CustomNodeSearchElement;
                var newId = workspace.CustomNodeDefinition.FunctionId;
                Assert.AreEqual(node.ID, newId);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeWorkspaceSavedToSamePlaceNotCausingDuplicatedLibraryItems()
        {
            // open file
            // save to the temporary folder
            // save to another temporary folder
            // save to the old temporary folder
            // there should be only two library items instead of three

            var dynamoModel = ViewModel.Model;
            var nodeName = Guid.NewGuid().ToString();
            var catName = "Custom Nodes";

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "", null);

            var tempPath1 = Path.Combine(TempFolder, nodeName + ".dyf");

            // Create the folder first incase it does not exist
            System.IO.Directory.CreateDirectory(Path.Combine(TempFolder, nodeName, nodeName));
            var tempPath2 = Path.Combine(TempFolder, nodeName, nodeName + ".dyf");

            def.Save(tempPath1);
            Thread.Sleep(1);

            def.Save(tempPath2);
            Thread.Sleep(1);

            def.Save(tempPath2);

            var count = dynamoModel.SearchModel.SearchEntries.OfType<CustomNodeSearchElement>().Where(
                            x => string.CompareOrdinal(x.Name, nodeName) == 0).Count();
            Assert.AreEqual(count, 2);
        }

        [Test]
        public void CustomNodeBelongingToPackageCanBeModifiedAndReopenedWithoutError()
        {

            // load package A
            // place custom node instance belonging to package A in homeworkspace
            // modify custom node
            // save custom node to same path
            // close custom node
            // open custom node
            // assert instances changed their number of output ports.
            var packageDirectory = Path.Combine(TestDirectory, "pkgs", "PackageThatWillBeModified");
            var loader = this.ViewModel.Model.GetPackageManagerExtension().PackageLoader;

           var package= Package.FromDirectory(packageDirectory, this.ViewModel.Model.Logger);
            loader.LoadPackages(new Package[] { package });

            //assert that package has been loaded.
            var foundPackage = loader.LocalPackages.Where(x => x.Name == "PackageThatWillBeModified").FirstOrDefault();
            Assert.IsNotNull(package);
            Assert.IsTrue(package.Loaded);
            //find our custom node
            var customNodeInfo = this.ViewModel.Model.CustomNodeManager.NodeInfos.Where(x => x.Value.Name == "ANodeToModify").FirstOrDefault();
            Assert.IsNotNull(customNodeInfo);

            //place an instance.
            var customNodeInstance = this.ViewModel.Model.CustomNodeManager.CreateCustomNodeInstance(customNodeInfo.Key);
            var oldNumPorts = customNodeInstance.OutPorts.Count();
            this.ViewModel.CurrentSpace.AddAndRegisterNode(customNodeInstance);
            Assert.AreEqual(1, this.ViewModel.Model.CurrentWorkspace.Nodes.OfType<Function>().Count());

            // open the custom node
            ViewModel.GoToWorkspaceCommand.Execute(customNodeInfo.Key);

            //add a new output node
            Assert.IsAssignableFrom(typeof(CustomNodeWorkspaceModel), this.ViewModel.Model.CurrentWorkspace);
            var newoutput = new Output();
            newoutput.Symbol = "anewoutput";
            this.ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(newoutput);

            //save the node, update the nodeInfo
            this.ViewModel.Model.CurrentWorkspace.Save(customNodeInfo.Value.Path);

            //swtich back to the home workspace
            ViewModel.Model.ExecuteCommand(new DynamoModel.SwitchTabCommand(0));
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), this.ViewModel.Model.CurrentWorkspace);

            var nodeInstance = this.ViewModel.Model.CurrentWorkspace.Nodes.OfType<Function>().FirstOrDefault();
            Assert.AreEqual(oldNumPorts+1, nodeInstance.OutPorts.Count());
            Assert.IsTrue(nodeInstance.OutPorts.LastOrDefault().Name.StartsWith("anewoutput"));

        }

        [Test]
        public void CustomNodeWorkspaceViewTestAfterSaving()
        {
            ViewModel.Model.OpenFileFromPath(Path.Combine(TestDirectory, @"core\combine", "Sequence_Json.dyf"));

            var newFilePath = GetNewFileNameOnTempPath("dyf");
            ViewModel.SaveAs(newFilePath, SaveContext.SaveAs);

            var oldJSON = File.ReadAllText(newFilePath);
            var oldJObject = JObject.Parse(oldJSON);

            var newJSON = File.ReadAllText(newFilePath);
            var newJObject = JObject.Parse(newJSON);

            Assert.AreEqual(oldJObject["View"], newJObject["View"]);
        }
        #endregion
    }
}
