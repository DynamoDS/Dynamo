using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Dynamo.Tests.Engine
{
    [TestFixture]
    class SyncDataManagerTest : DynamoModelTestBase
    {
        /// <summary>
        /// The next test method will execute the  public SyncDataManager Clone() methof from the SyncDataManager class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void SyncDataManagerCloneTest()
        {
            //Arrange
            var openPath = Path.Combine(TestDirectory, @"core\nodeLocationTest.dyn");
            RunModel(openPath);
            var nodesList = CurrentDynamoModel.CurrentWorkspace.Nodes;

            //Act
            //Internally the PreviewGraphSyncData calls the SyncDataManager.Clone() method
            CurrentDynamoModel.EngineController.PreviewGraphSyncData(nodesList, true);

            //Assert
            //Verify that at least we have 1 node
            Assert.Greater(nodesList.Count(), 0);
           
        }
    }
}
