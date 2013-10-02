using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Tests;
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
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsNewFile()
        {
            // new custom node
            // save as
            // file exists
            Assert.Fail();
        }

        [Test]
        public void HomeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists
            Assert.Fail();
        }

        [Test]
        public void CustomNodeWorkspaceCanSaveAsExistingFile()
        {
            // open file
            // save as
            // file exists
            Assert.Fail();
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
