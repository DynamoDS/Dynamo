using NUnit.Framework;
using System.IO;
using Dynamo.Migration;
using Dynamo.Models;
using Dynamo.Graph.Nodes;
using System;

namespace Dynamo.Tests.Migrations
{ 
    [TestFixture]
    class MigrationReportTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will call the next methods:
        /// MigrationReport.AddMigrationDataToNodeMap
        /// MigrationReport.WriteToXmlFile
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestWorkspaceMigrationAttribute()
        {
            //Arrange
            DynamoModel.EnableMigrationLogging = true;
            string openPath = Path.Combine(TestDirectory, @"core\nodeLocationTest.dyn");

            //Act
            // Open/Run XML test graph          
            RunModel(openPath);

            // Save/Open/Run JSON graph
            string tempPath = Path.Combine(Path.GetTempPath(), "nodeLocationTest.dyn");
            CurrentDynamoModel.CurrentWorkspace.Save(tempPath);
            CurrentDynamoModel.OpenFileFromPath(tempPath);
            CurrentDynamoModel.CurrentWorkspace.RequestRun();

            //Assert
            //Check that the file exists
            Assert.IsTrue(File.Exists(Path.Combine(TestDirectory, @"core\MigrationLog_nodeLocationTest.xml")));//Validate the MigrationReport.WriteToXmlFile

            // Delete temp graph file
            File.Delete(tempPath);
        }
    }
}
