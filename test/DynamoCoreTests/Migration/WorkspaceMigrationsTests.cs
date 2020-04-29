using Dynamo.Migration;
using NUnit.Framework;
using System.IO;
using System.Xml;

namespace Dynamo.Tests.Migrations
{
    [TestFixture]
    class WorkspaceMigrationsTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will create an instance of the WorkspaceMigrations and check the function Migrate_0_5_3_to_0_6_0
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestWorkspaceMigrations()
        {
            //Arrange
            string openPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            RunModel(openPath);
            string logFileText = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();

            //Act
            //Create a instance of WorkspaceMigration and call the function that writes to the log
            var workMigration = new WorkspaceMigrations(CurrentDynamoModel);
            workMigration.Migrate_0_5_3_to_0_6_0(xmlDoc);
            var currentLoggerPath = CurrentDynamoModel.Logger.LogPath;

            //Assert
            Assert.IsTrue(File.Exists(currentLoggerPath));//Check that the log was created successfully
            Assert.IsTrue(CurrentDynamoModel.Logger.LogText.Contains("migration from 0.5.3.x to 0.6.1.x"));//Checks that the log text contains the migration info
        }
    }
}
