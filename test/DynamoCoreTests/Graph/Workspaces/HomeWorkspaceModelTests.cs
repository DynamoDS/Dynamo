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
        private bool eventWasRaised;
        private string objectFileName;

        public void OpenTestFile(string folder, string fileName)
        {
            var examplePath = Path.Combine(TestDirectory, folder, fileName);
            OpenModel(examplePath);
        }

        [SetUp]
        public void Init()
        {
            eventWasRaised = false;
            objectFileName = "";
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
            homeWorkspace.SetNodeDeltaState += EventHandler;

            homeWorkspace.Run();
            homeWorkspace.GetExecutingNodes(true);

            homeWorkspace.SetNodeDeltaState -= EventHandler;

            Assert.IsTrue(eventWasRaised);
            Assert.AreEqual(homeWorkspace.FileName, objectFileName);
        }

        private void EventHandler (object workspace, DeltaComputeStateEventArgs d)
        {
            eventWasRaised = true;
            var eventParamWorkspace = workspace as HomeWorkspaceModel;
            objectFileName = eventParamWorkspace.FileName;
        }
    }
}
