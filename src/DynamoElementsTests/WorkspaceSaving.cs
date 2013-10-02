using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo
{
    class WorkspaceSaving : DynamoUnitTest
    {

        [Test]
        public void HomeWorkspaceCanSaveAsNewFile()
        {
            // new home workspace
            // save as
            // file exists  

            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull( dynamoModel.CurrentWorkspace );
            Assert.IsAssignableFrom( typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace );

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsNewFile()
        {
            // new custom node
            // save as
            // file exists

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

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

            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math", "Add.dyn");
            model.Open(examplePath);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists

            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            model.Open(examplePath);

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

            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math", "Add.dyn");
            model.Open(examplePath);

            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs("");

            Assert.IsFalse(res);
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsFailsWithoutFilepathArg()
        {
            // open file
            // save as
            // file exists

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var res = def.WorkspaceModel.SaveAs(null);

            Assert.IsFalse(res);
        }

        [Test]
        public void HomeWorkspaceSaveFailsWithoutFilepathProperty()
        {
            // new file
            // save fails

            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var res = Controller.DynamoModel.CurrentWorkspace.Save();

            Assert.IsFalse(res);
        }

        [Test]
        public void CustomNodeWorkspacSaveFailsWithoutFilepathProperty()
        {
            // new file
            // save fails

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var res = def.WorkspaceModel.Save();

            Assert.IsFalse(res);
        }

        [Test]
        public void HomeWorkspaceFilePathPropertyIsUpdatedOnSaveAs()
        {
            // open file
            // save as
            // FileName is updated

            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
        }

        [Test]
        public void CustomNodeWorkspaceFilePathIsUpdatedOnSaveAs()
        {
            // open file
            // save as
            // FileName is updated

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = def.WorkspaceModel.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));

            Assert.AreEqual(newPath, def.WorkspaceModel.FileName );
        }

        [Test]
        public void HomeWorkspaceCanSaveAsMultipleTimes()
        {
            // open file
            // for x 10
            // save as
            // file exists, filepath updated

            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            foreach (var i in Enumerable.Range(0, 10))
            {
                var newPath = this.GetNewFileNameOnTempPath("dyn");
                var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

                Assert.IsTrue(res);
                Assert.IsTrue(File.Exists(newPath));
                Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
            }
           
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsMultipleTimes()
        {
            // open file
            // for x 10
            // save as
            // file exists, filepath updated 

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

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
        public void HomeWorkspaceCanSaveAsAndThenSaveExistingFile()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save
            // file is updated

            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);

            var resSave = Controller.DynamoModel.CurrentWorkspace.Save();

            Assert.IsTrue(resSave);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
            Thread.Sleep(1);
            var saveTime = File.GetLastWriteTime(newPath);

            // assert the file has new update
            
            Assert.Greater(saveTime.Ticks - saveAsTime.Ticks, 0);
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsAndThenSaveExistingFile()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save
            // file is updated

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = def.WorkspaceModel.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, def.WorkspaceModel.FileName);
            var saveAsTime = File.GetLastWriteTime(newPath);


            var resSave = def.WorkspaceModel.Save();

            Assert.IsTrue(resSave);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
            var saveTime = File.GetLastWriteTime(newPath);

            // assert the file has new update
            Assert.Greater(saveTime.Ticks - saveAsTime.Ticks, 0);
        }

        [Test]
        public void HomeWorkspaceCanSaveMultipleTimes()
        {
            // open file
            // save
            // file is updated
            // save
            // file is updated

            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
            var lastSaveTime = File.GetLastWriteTime(newPath);

            foreach (var i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(1);
                var resSave = Controller.DynamoModel.CurrentWorkspace.Save();

                Assert.IsTrue(resSave);
                Assert.IsTrue(File.Exists(newPath));
                Assert.AreEqual(newPath, Controller.DynamoModel.CurrentWorkspace.FileName);
                var saveTime = File.GetLastWriteTime(newPath);

                // assert the file has new update
                Assert.Greater(saveTime.Ticks - lastSaveTime.Ticks, 0);
            }
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveMultipleTimes()
        {
            // open file
            // save
            // file is updated
            // save
            // file is updated

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

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
        public void HomeWorkspaceHasUnsavedChangesPropertyIsSetOnSaveAs()
        {
            // open file
            // make change
            // saveAs
            // SavedProperty is true, filePath set, file exists

            // get empty workspace
            var dynamoModel = Controller.DynamoModel;
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            // make change
            dynamoModel.CreateNode(new Dictionary<string, object>()
                {
                    {"name", "Add" },
                    {"x", 0 },
                    {"y", 0 }
                });
            Assert.IsTrue(Controller.DynamoModel.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(1, Controller.DynamoModel.CurrentWorkspace.Nodes.Count);

            // save
            var newPath = this.GetNewFileNameOnTempPath("dyn");
            Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            // check expected
            Assert.IsFalse(Controller.DynamoModel.CurrentWorkspace.HasUnsavedChanges);

        }

        [Test]
        public void CustomNodeWorkspaceHasUnsavedChangesPropertyIsSetOnSaveAs()
        {
            // open file
            // make change
            // saveAs
            // SavedProperty is true, filePath set, file exists

            var dynamoModel = Controller.DynamoModel;
            var nodeName = "Cool node";
            var catName = BuiltinNodeCategories.SCRIPTING_CUSTOMNODES;

            var def = dynamoModel.NewCustomNodeWorkspace(Guid.NewGuid(), nodeName, catName, "", true);
            Assert.IsFalse(def.WorkspaceModel.HasUnsavedChanges);

            dynamoModel.CreateNode(new Dictionary<string, object>()
                {
                    {"name", "Add" },
                    {"x", 0 },
                    {"y", 0 }
                });

            Assert.IsTrue(def.WorkspaceModel.HasUnsavedChanges);
            Assert.AreEqual(1, def.WorkspaceModel.Nodes.Count );
            
            var newPath = this.GetNewFileNameOnTempPath("dyf");
            def.WorkspaceModel.SaveAs(newPath);

            Assert.IsFalse(def.WorkspaceModel.HasUnsavedChanges);

        }

        #region CustomNodeWorkspaceModel SaveAs side effects

        [Test]
        public void CustomNodeSaveAsGivesNewFunctionId()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // custom node instance is in environment
            Assert.Inconclusive("Not finished");
        }

        [Test]
        public void CustomNodeSaveAsAddsNewIdToEnvironment()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // function id is in environment
            Assert.Inconclusive("Not finished");
        }

        [Test]
        public void CustomNodeSaveAsCanBeUsedInWorkspaceAndExpressionRuns()
        {
            // open custom node
            // SaveAs
            // place custom node with new id, run expression and result is correct.
            Assert.Inconclusive("Not finished");
        }

        [Test]
        public void CustomNodeSaveAsAddsNewLoadedNodeToCustomNodeManager()
        {
            // open custom node
            // SaveAs
            // new node with new function id in custom node manager with save name
            // can get instance of that node
            Assert.Inconclusive("Not finished");
        }

        [Test]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearch()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, difer 
            Assert.Inconclusive("Not finished");
        }

        [Test]
        public void CustomNodeSaveAsPreservesFunctionDefinitionsOfExistingInstances()
        {
            // open custom node
            // place the custom node a few times in home workspace
            // SaveAs
            // can get instances of original custom node
            Assert.Inconclusive("Not finished");
        }

        #endregion
    }
}
