using System.IO;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the HomeWorkspaceModel class
    /// </summary>
    [TestFixture]
    class HomeWorkspaceModelTests : DynamoModelTestBase
    {
        public void OpenTestFile(string folder, string fileName)
        {
            var examplePath = Path.Combine(TestDirectory, folder, fileName);
            OpenModel(examplePath);
        }

        [Test]
        [Category("UnitTests")]
        public void OnPreviewGraphCompletedTest()
        {
            //Loads a file with a valid graph on the Home workspace
            OpenTestFile(@"core\number", "TestBigNumber.dyn");
            var homeWorkspace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);

            //Gets results from raised event
            bool eventWasRaised = false;
            string objectFileName = "";
            homeWorkspace.SetNodeDeltaState += delegate(object workspace, DeltaComputeStateEventArgs d)
            {
                eventWasRaised = true;
                var eventParamWorkspace = workspace as HomeWorkspaceModel;
                objectFileName = eventParamWorkspace.FileName;
            };

            homeWorkspace.Run();
            homeWorkspace.GetExecutingNodes(true);

            Assert.IsTrue(eventWasRaised);
            Assert.AreEqual(homeWorkspace.FileName, objectFileName);
        }
    }
}
