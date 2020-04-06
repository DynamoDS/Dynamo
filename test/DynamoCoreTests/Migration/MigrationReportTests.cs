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
            string openPath = string.Empty;
            string tempPath = string.Empty;

            try
            {
                openPath = Path.Combine(TestDirectory, @"core\nodeLocationTest.dyn");
                tempPath = Path.Combine(Path.GetTempPath(), "nodeLocationTest.dyn");

                //Act
                // Open/Run XML test graph          
                RunModel(openPath);

                // Save/Open/Run JSON graph              
                CurrentDynamoModel.CurrentWorkspace.Save(tempPath);
                CurrentDynamoModel.OpenFileFromPath(tempPath);
                CurrentDynamoModel.CurrentWorkspace.RequestRun();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            } 
            finally
            {
                //Assert
                //This flag needs to be set to false because if not the next test cases will start to fail
                DynamoModel.EnableMigrationLogging = false;

                //Check that the file exists
                Assert.IsTrue(File.Exists(Path.Combine(TestDirectory, @"core\MigrationLog_nodeLocationTest.xml")));//Validate the MigrationReport.WriteToXmlFile

                if (File.Exists(tempPath))
                {
                    // Delete temp graph file
                    File.Delete(tempPath);
                }

            }
            
        }
    }
}
