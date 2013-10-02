using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            
            Assert.IsNotNull(dynamoModel.CurrentWorkspace);
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), dynamoModel.CurrentWorkspace);

            var newPath = this.GetNewFileNameOnTempPath("dyn");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

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
            var examplePath = Path.Combine(GetTestDirectory(), @"core\math");

            string openPath = Path.Combine(examplePath, "Add.dyn");
            model.Open(openPath);

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
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine");

            var openPath = Path.Combine(examplePath, "Sequence2.dyn");
            model.Open(openPath);

            var newPath = this.GetNewFileNameOnTempPath("dyf");
            var res = Controller.DynamoModel.CurrentWorkspace.SaveAs(newPath);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsFailsWithoutFilepathArg()
        {
            // open file
            // save as
            // file exists
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceCanSaveAsFailsWithoutFilepathArg()
        {
            // open file
            // save as
            // file exists
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceSaveFailsWithoutFilepathProperty()
        {
            // new file
            // save fails
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspacSaveFailsWithoutFilepathProperty()
        {
            // new file
            // save fails
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceFilePathPropertyIsUpdatedOnSaveAs()
        {
            // open file
            // save as
            // FilePath is updated
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceFilePathIsUpdatedOnSaveAs()
        {
            // open file
            // save as
            // FilePath is updated
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceCanSaveAsMultipleTimes()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save as
            // file exists, filepath updated
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsMultipleTimes()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save as
            // file exists, filepath updated
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceCanSaveAsAndThenSaveExistingFile()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save
            // file is updated
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsAndThenSaveExistingFile()
        {
            // open file
            // save as
            // file exists, filepath updated
            // save
            // file is updated
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceCanSaveMultipleTimes()
        {
            // open file
            // save
            // file is updated
            // save
            // file is updated
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveMultipleTimes()
        {
            // open file
            // save
            // file is updated
            // save
            // file is updated
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceSavedPropertyIsSetOnSaveAs()
        {
            // open file
            // make change
            // saveAs
            // SavedProperty is true, filePath set, file exists
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceSavedPropertyIsSetOnSaveAs()
        {
            // open file
            // make change
            // saveAs
            // SavedProperty is true, filePath set, file exists
            Assert.Fail();
        }


        #region CustomNodeWorkspaceModel SaveAs side effects

        [Test]
        public void CustomNodeSaveAsGivesNewFunctionId()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // custom node instance is in environment
            Assert.Fail();
        }

        [Test]
        public void CustomNodeSaveAsAddsNewIdToEnvironment()
        {
            // open custom node
            // SaveAs
            // custom node instance has new function id
            // function id is in environment
            Assert.Fail();
        }

        [Test]
        public void CustomNodeSaveAsCanBeUsedInWorkspaceAndExpressionRuns()
        {
            // open custom node
            // SaveAs
            // place custom node with new id, run expression and result is correct.
            Assert.Fail();
        }

        [Test]
        public void CustomNodeSaveAsAddsNewLoadedNodeToCustomNodeManager()
        {
            // open custom node
            // SaveAs
            // new node with new function id in custom node manager with save name
            // can get instance of that node
            Assert.Fail();
        }

        [Test]
        public void CustomNodeSaveAsAddsNewCustomNodeToSearch()
        {
            // open custom node
            // SaveAs
            // two nodes are returned in search on custom node name, difer 
            Assert.Fail();
        }

        [Test]
        public void CustomNodeSaveAsPreservesFunctionDefinitionsOfExistingInstances()
        {
            // open custom node
            // place the custom node a few times in home workspace
            // SaveAs
            // can get instances of original custom node
            Assert.Fail();
        }

        #endregion
    }
}
