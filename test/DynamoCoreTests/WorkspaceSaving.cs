using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Tests;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels;

using NUnit.Framework;
using Dynamo.ViewModels;

namespace Dynamo.Tests
{
    public class WorkspaceSaving : DynamoViewModelUnitTest
    {

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
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
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
            Assert.AreEqual(0, workspace.Nodes.Count); // An empty workspace

            var addNode = new DSFunction(dynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var createNodeCommand = new DynamoModel.CreateNodeCommand(
                addNode, 0, 0, false, false);

            // Create a new node in the empty workspace.
            ViewModel.ExecuteCommand(createNodeCommand);
            Assert.AreEqual(1, workspace.Nodes.Count);

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
            var res = ws.SaveAs(newPath, dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

        }

        [Test]
        public void HomeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists

            var examplePath = Path.Combine(GetTestDirectory(), @"core\math", "Add.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var newPath = GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);

            Assert.IsNotNull(nodeWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        public void HomeWorkspaceCanSaveAsFailsWithoutFilepathArg()
        {
            // open file
            // save as
            // file exists

            var examplePath = Path.Combine(GetTestDirectory(), @"core\math", "Add.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var res = ViewModel.Model.CurrentWorkspace.SaveAs("", ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsFalse(res);
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

            var res = def.SaveAs(null, dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsFalse(res);
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

            var res = ViewModel.Model.CurrentWorkspace.Save(dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsFalse(res);
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

            var res = def.Save(dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsFalse(res);
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
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
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

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = def.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
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
                var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

                Assert.IsTrue(res);
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

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");

            foreach (var i in Enumerable.Range(0, 10))
            {
                var newPath = GetNewFileNameOnTempPath("dyf");
                var res = def.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

                Assert.IsTrue(res);
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
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            Thread.Sleep(1);
            var resSave = ViewModel.Model.CurrentWorkspace.Save(dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsTrue(resSave);
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

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = def.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            Thread.Sleep(1);
            var resSave = def.Save(dynamoModel.EngineController.LiveRunnerCore);

            Assert.IsTrue(resSave);
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
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                var resSave = ViewModel.Model.CurrentWorkspace.Save(dynamoModel.EngineController.LiveRunnerCore);

                Assert.IsTrue(resSave);
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

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = def.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                var resSave = def.Save(dynamoModel.EngineController.LiveRunnerCore);

                Assert.IsTrue(resSave);
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
            dynamoModel.CurrentWorkspace.AddNode(node, false);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count);

            // save
            var newPath = GetNewFileNameOnTempPath("dyn");
            ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            // check expected
            Assert.IsFalse(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);

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

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");
            Assert.IsFalse(def.HasUnsavedChanges);

            var node = new DSFunction(dynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            def.AddNode(node, false);
            Assert.IsTrue(def.HasUnsavedChanges);
            Assert.AreEqual(1, def.Nodes.Count );
            
            var newPath = GetNewFileNameOnTempPath("dyf");
            def.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsFalse(def.HasUnsavedChanges);
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
            workspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            var newDef = workspace.CustomNodeId;

            Assert.AreNotEqual(Guid.Empty, initialId);
            Assert.AreNotEqual(Guid.Empty, newDef);
            Assert.AreNotEqual(initialId, newDef);
        }

        [Test]
        public void CustomNodeSaveAsGivesNewFunctionIdToExistingFile()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // custom node instance is in environment

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            Assert.IsTrue(res);
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
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.OfType<CustomNodeWorkspaceModel>().FirstOrDefault();
            
            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            // workspace now has different function id
            var newDef = nodeWorkspace.CustomNodeDefinition;
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.IsInitialized(newDef.FunctionId));
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.Contains(oldId));
        }

        [Test]
        public void CustomNodeSaveAsAddsNewIdToEnvironmentAndMaintainsOldOne()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // function id is in environment

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = GetNewFileNameOnTempPath("dyf");
            nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            var newDef = nodeWorkspace.CustomNodeDefinition;
        }

        [Test]
        public void CustomNodeSaveAsNewCustomNodeCanBeUsedInWorkspaceAndExpressionRuns()
        {
            // open custom node
            // SaveAs
            // place custom node with new id, run expression and result is correct.

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);


            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);

            var newPath = GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            // get new function id

            // put in workspace
            model.CurrentWorkspace = ViewModel.HomeSpace;
            var newCustNodeInstance =
                model.CustomNodeManager.CreateCustomNodeInstance(nodeWorkspace.CustomNodeId);
            model.CurrentWorkspace.AddNode(newCustNodeInstance, false);

            // run expression
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);

            // run expression is correct
            ViewModel.HomeSpace.Run();

            var evaluatedNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            Assert.AreEqual(2.0, evaluatedNode.CachedValue.Data);
        }

        [Test]
        [Category("Failure")]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearch()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, difer 

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = Path.Combine(TempFolder, "Constant2.dyf");
            var originalNumElements = ViewModel.Model.SearchModel.NumElements;
            nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            ViewModel.SearchViewModel.SearchAndUpdateResults("Constant2");
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.NumElements);

            Assert.AreEqual(2, ViewModel.SearchViewModel.SearchResults.Count);

            var res1 = ViewModel.SearchViewModel.SearchResults[0];
            var res2 = ViewModel.SearchViewModel.SearchResults[1];

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
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
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
                model.CurrentWorkspace.AddNode(node, false);
            }

            // SaveAs
            var newPath = GetNewFileNameOnTempPath("dyf");
            var newName = Path.GetFileNameWithoutExtension(newPath);
            var res = nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            Assert.IsNotNull(nodeWorkspace.CustomNodeDefinition);

            // can get instances of original custom node
            Assert.AreEqual(10, homeWorkspace.Nodes.Count);
            var funcs =
                homeWorkspace.Nodes.OfType<Function>()
                    .Where(x => x.Definition.FunctionId == nodeWorkspace.CustomNodeId)
                    .ToList();
            Assert.AreEqual(10, funcs.Count);
            funcs.ForEach(x => Assert.AreEqual(newName, x.NickName));
            
        }

        [Test]
        [Category("Failure")]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearchAndItCanBeRefactoredWhilePreservingOriginalFromExistingDyf()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, difer 

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var oldId = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>().Definition.FunctionId;

            CustomNodeWorkspaceModel nodeWorkspace;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(oldId, true, out nodeWorkspace));
            
            var newPath = GetNewFileNameOnTempPath("dyf");
            var originalNumElements = ViewModel.Model.SearchModel.NumElements;

            // save as
            nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            CustomNodeWorkspaceModel oldWs;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(oldId, true, out oldWs));

            // refactor oldId with new name
            oldWs.SetInfo("TheNoodle", "TheCat", "TheCat");

            // num elements is unchanged by refactor
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.NumElements);

            // search for refactored node
            ViewModel.SearchViewModel.SearchAndUpdateResults("TheNoodle");

            // results are correct
            Assert.AreEqual(1, ViewModel.SearchViewModel.SearchResults.Count);
            var node3 = (CustomNodeSearchElement)ViewModel.SearchViewModel.SearchResults[0].Model;
            Assert.AreEqual(newId, node3.ID);

            // search for un-refactored node
            ViewModel.SearchViewModel.SearchAndUpdateResults("Constant2");

            // results are correct
            Assert.AreEqual(1, ViewModel.SearchViewModel.SearchResults.Count);
            var node4 = (CustomNodeSearchElement)ViewModel.SearchViewModel.SearchResults[0].Model;
            Assert.AreEqual(oldId, node4.ID);

        }

        [Test]
        [Category("Failure")]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearchAndItCanBeRefactoredWhilePreservingOriginalFromExistingDyf2()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, difer 

            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_saving", "Constant2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var oldId = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>().Definition.FunctionId;

            CustomNodeWorkspaceModel nodeWorkspace;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(oldId, true, out nodeWorkspace));

            var newPath = GetNewFileNameOnTempPath("dyf");
            var originalNumElements = ViewModel.Model.SearchModel.NumElements;

            // save as
            nodeWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            // refactor oldId with new name

            CustomNodeWorkspaceModel oldWs;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(oldId, true, out oldWs));

            // refactor oldId with new name
            oldWs.SetInfo("Constant2 Alt", "TheCat", "TheCat");

            // num elements is unchanged by refactor
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.NumElements);

            // search common base name
            ViewModel.SearchViewModel.SearchAndUpdateResults("Constant2");

            // results are correct
            Assert.AreEqual(2, ViewModel.SearchViewModel.SearchResults.Count);

            var res1 = ViewModel.SearchViewModel.SearchResults[0];
            var res2 = ViewModel.SearchViewModel.SearchResults[1];

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

            var def = dynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName, "");
            var workspace = (CustomNodeWorkspaceModel)def;

            var listGuids = new List<Guid>();
            var listNames = new List<string>();

            for (var i = 0; i < 10; i++)
            {
                var newPath = GetNewFileNameOnTempPath("dyf");
                workspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

                var newId = workspace.CustomNodeDefinition.FunctionId;
                var newName = workspace.Name;

                listGuids.Add(newId);
                listNames.Add(newName);

                listGuids.ForEach( x => Assert.IsTrue( ViewModel.Model.CustomNodeManager.NodeInfos.ContainsKey(x) ));
                listNames.ForEach( x => Assert.IsTrue( ViewModel.Model.CustomNodeManager.Contains(x) ));
            }
        }

        #endregion
    }
}
