using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
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

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

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

            var createNodeCommand = new DynamoModel.CreateNodeCommand(
                Guid.NewGuid(), "Add", 0, 0, false, false);

            // Create a new node in the empty workspace.
            ViewModel.ExecuteCommand(createNodeCommand);
            Assert.AreEqual(1, workspace.Nodes.Count);

            Assert.AreEqual(true, workspace.CanUndo);
            Assert.AreEqual(false, workspace.CanRedo);
            dynamoModel.CleanWorkbench(); // Clearing current workspace.

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);
                
            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = def.WorkspaceModel.SaveAs(newPath);

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

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

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

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath);

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

            var res = ViewModel.Model.CurrentWorkspace.SaveAs("");

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var res = def.WorkspaceModel.SaveAs(null);

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

            var res = ViewModel.Model.CurrentWorkspace.Save();

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var res = def.WorkspaceModel.Save();

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

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = def.WorkspaceModel.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            Assert.AreEqual(newPath, def.WorkspaceModel.FileName );
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
                var newPath = this.GetNewFileNameOnTempPath("dyn");
                var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            foreach (var i in Enumerable.Range(0, 10))
            {
                var newPath = this.GetNewFileNameOnTempPath("dyf");
                var res = def.WorkspaceModel.SaveAs(newPath);

                Assert.IsTrue(res);
                Assert.IsTrue(File.Exists(newPath));

                Assert.AreEqual(newPath, def.WorkspaceModel.FileName);
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

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            Thread.Sleep(1);
            var resSave = ViewModel.Model.CurrentWorkspace.Save();

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = def.WorkspaceModel.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.WorkspaceModel.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            Thread.Sleep(1);
            var resSave = def.WorkspaceModel.Save();

            Assert.IsTrue(resSave);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
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

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, ViewModel.Model.CurrentWorkspace.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                var resSave = ViewModel.Model.CurrentWorkspace.Save();

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = def.WorkspaceModel.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.WorkspaceModel.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                var resSave = def.WorkspaceModel.Save();

                Assert.IsTrue(resSave);
                Assert.IsTrue(File.Exists(newPath));
                Assert.AreEqual(newPath, def.WorkspaceModel.FileName);
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
            dynamoModel.CurrentWorkspace.AddNode(0.0, 0.0, "Add");
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count);

            // save
            var newPath = this.GetNewFileNameOnTempPath("dyn");
            ViewModel.Model.CurrentWorkspace.SaveAs(newPath);

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

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);
            Assert.IsFalse(def.WorkspaceModel.HasUnsavedChanges);

            dynamoModel.CurrentWorkspace.AddNode(0.0, 0.0, "Add");
            Assert.IsTrue(def.WorkspaceModel.HasUnsavedChanges);
            Assert.AreEqual(1, def.WorkspaceModel.Nodes.Count );
            
            var newPath = this.GetNewFileNameOnTempPath("dyf");
            def.WorkspaceModel.SaveAs(newPath);

            Assert.IsFalse(def.WorkspaceModel.HasUnsavedChanges);

        }

        #region CustomNodeWorkspaceModel SaveAs side effects

        [Test]
        [Category("UnitTests")]
        public void CustomNodeSaveAsDoesNotGiveNewFunctionIdToNewCustomNode()
        {
            // new custom node
            // SaveAs
            // custom node instance has new function id
            // custom node instance is in environment

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);
            var workspace = def.WorkspaceModel;
            var initialId = def.FunctionId;

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            workspace.SaveAs(newPath);

            var newDef = workspace.CustomNodeDefinition;

            Assert.AreNotEqual(Guid.Empty, initialId);
            Assert.AreNotEqual(Guid.Empty, newDef.FunctionId);
            Assert.AreEqual(newDef.FunctionId, initialId);

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

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath); // introduces new function id

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
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;
            
            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath); // introduces new function id

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            // workspace now has different function id
            var newDef = nodeWorkspace.CustomNodeDefinition;
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.IsInitialized(newDef.FunctionId));
            Assert.IsTrue(ViewModel.Model.CustomNodeManager.IsInitialized(oldId));
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

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            nodeWorkspace.SaveAs(newPath);

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

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath); // introduces new function id

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            // get new function id
            var newDef = nodeWorkspace.CustomNodeDefinition;

            // put in workspace
            model.Home(null);
            model.CurrentWorkspace.AddNode(0.0, 0.0, newDef.FunctionId.ToString());

            // run expression
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);

            // run expression is correct
            ViewModel.Model.RunExpression();

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
            var originalNumElements = ViewModel.Model.SearchModel.SearchDictionary.NumElements;
            nodeWorkspace.SaveAs(newPath); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            ViewModel.SearchViewModel.SearchAndUpdateResults("Constant2");
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.SearchDictionary.NumElements);

            Assert.AreEqual(2, ViewModel.SearchViewModel.SearchResults.Count);

            var res1 = ViewModel.SearchViewModel.SearchResults[0];
            var res2 = ViewModel.SearchViewModel.SearchResults[1];

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res1);
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res2);

            var node1 = res1.Model as CustomNodeSearchElement;
            var node2 = res2.Model as CustomNodeSearchElement;

            Assert.IsTrue((node1.Guid == oldId && node2.Guid == newId) ||
                          (node1.Guid == newId && node2.Guid == oldId));

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
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            // place the custom node a few times in home workspace
            var homeWorkspace = model.Workspaces.OfType<HomeWorkspaceModel>().First();
            model.CurrentWorkspace = homeWorkspace;
            foreach (var i in Enumerable.Range(0, 10))
                model.CurrentWorkspace.AddNode(0.0, 0.0, oldId.ToString());
            
            // SaveAs
            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = nodeWorkspace.SaveAs(newPath); // introduces new function id

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            Assert.IsNotNull(nodeWorkspace.CustomNodeDefinition);

            // can get instances of original custom node
            Assert.AreEqual(10, homeWorkspace.Nodes.Count);
            var funcs = homeWorkspace.Nodes.OfType<Function>().Where(x => x.Definition.FunctionId == oldId).ToList();
            Assert.AreEqual(10, funcs.Count);
            funcs.ForEach(x => Assert.AreEqual( "Constant2", x.Name ));
            
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

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var originalNumElements = ViewModel.Model.SearchModel.SearchDictionary.NumElements;

            // save as
            nodeWorkspace.SaveAs(newPath); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            // refactor oldId with new name

            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var dummyInfo1 = new CustomNodeInfo(newId, nodeName, catName, descr, "");
            ViewModel.Model.CustomNodeManager.Refactor(dummyInfo1);

            // num elements is unchanged by refactor
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.SearchDictionary.NumElements);

            // search for refactored node
            ViewModel.SearchViewModel.SearchAndUpdateResults("TheNoodle");

            // results are correct
            Assert.AreEqual(1, ViewModel.SearchViewModel.SearchResults.Count);
            var node3 = ViewModel.SearchViewModel.SearchResults[0].Model as CustomNodeSearchElement;
            Assert.AreEqual(newId, node3.Guid);

            // search for un-refactored node
            ViewModel.SearchViewModel.SearchAndUpdateResults("Constant2");

            // results are correct
            Assert.AreEqual(1, ViewModel.SearchViewModel.SearchResults.Count);
            var node4 = ViewModel.SearchViewModel.SearchResults[0].Model as CustomNodeSearchElement;
            Assert.AreEqual(oldId, node4.Guid);

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

            var nodeWorkspace =
                model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;

            Assert.IsNotNull(nodeWorkspace);
            var oldId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var originalNumElements = ViewModel.Model.SearchModel.SearchDictionary.NumElements;

            // save as
            nodeWorkspace.SaveAs(newPath); // introduces new function id

            var newId = nodeWorkspace.CustomNodeDefinition.FunctionId;

            // refactor oldId with new name

            var nodeName = "Constant2 Alt";
            var catName = "TheCat";
            var descr = "TheCat";
            var dummyInfo1 = new CustomNodeInfo(newId, nodeName, catName, descr, "");
            ViewModel.Model.CustomNodeManager.Refactor(dummyInfo1);

            // num elements is unchanged by refactor
            Assert.AreEqual(originalNumElements + 1, ViewModel.Model.SearchModel.SearchDictionary.NumElements);

            // search common base name
            ViewModel.SearchViewModel.SearchAndUpdateResults("Constant2");

            // results are correct
            Assert.AreEqual(2, ViewModel.SearchViewModel.SearchResults.Count);

            var res1 = ViewModel.SearchViewModel.SearchResults[0];
            var res2 = ViewModel.SearchViewModel.SearchResults[1];

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res1);
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElementViewModel), res2);

            var node1 = res1.Model as CustomNodeSearchElement;
            var node2 = res2.Model as CustomNodeSearchElement;

            Assert.IsTrue((node1.Guid == oldId && node2.Guid == newId) ||
                          (node1.Guid == newId && node2.Guid == oldId));
        }

        [Test]
        public void MultipleCustomNodeSaveAsOperationsAddsMultipleValidFunctionIdsToCustomNodeManager()
        {

            var dynamoModel = ViewModel.Model;
            var nodeName = "Cool node";
            var catName = "Custom Nodes";

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);
            var workspace = def.WorkspaceModel;

            var listGuids = new List<Guid>();
            var listNames = new List<string>();

            for (var i = 0; i < 10; i++)
            {
                var newPath = this.GetNewFileNameOnTempPath("dyf");
                workspace.SaveAs(newPath);

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
